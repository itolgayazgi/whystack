using WhyStack.Application.Abstractions;
using WhyStack.Application.Common;
using WhyStack.Application.Identity.Sessions;
using WhyStack.Domain.Identity;

namespace WhyStack.Application.Identity.Refresh;

public sealed record RefreshCommand(string? RefreshToken, SessionContext Context);

public sealed record RefreshResult(
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc,
    Guid UserId);

/// <summary>
/// Rotation with reuse detection. ADR-0008 calls this the high-risk area; `13` agrees.
/// </summary>
/// <remarks>
/// <b>Rotation alone is not the defence.</b> Replacing the token on every use shortens how long a
/// stolen one is useful, but nothing notices that a theft happened — the victim and the thief simply
/// take turns, and whoever refreshes last keeps the account.
///
/// <b>Detection is the defence.</b> A token that has already been rotated should never be seen again.
/// If it is, exactly one of two things is true: the attacker is replaying a stolen token, or the real
/// user is replaying one they should no longer hold. Both mean it leaked.
///
/// The response is not to reject that request. It is to <b>revoke the entire family</b> — every session
/// descended from that sign-in — because the thief may already hold a newer token in the chain, and
/// killing only the replayed one leaves them signed in while the victim is not.
///
/// The user is signed out of that device. That is the point: better a legitimate user signs in again
/// than an attacker stays signed in forever.
/// </remarks>
public sealed class RefreshHandler(
    IIdentityRepository repository,
    SessionService sessions,
    IAccessTokenIssuer accessTokenIssuer,
    IClock clock)
{
    /// <summary>
    /// Every failure looks the same. A refresh token is a bearer credential — telling the caller
    /// whether it was expired, revoked or replayed tells an attacker how close they are.
    /// </summary>
    private static readonly Error InvalidRefreshToken = new(
        ErrorCodes.InvalidRefreshToken,
        "The session has ended. Please sign in again.");

    public async Task<Result<RefreshResult>> HandleAsync(
        RefreshCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.RefreshToken))
        {
            return InvalidRefreshToken;
        }

        var now = clock.UtcNow;
        var session = await sessions.FindByRawTokenAsync(command.RefreshToken, cancellationToken);

        if (session is null)
        {
            // A token we have never issued, or one whose row is long gone. Nothing to revoke.
            await RecordAsync(null, null, LoginEventType.TokenRefreshFailed, "unknown_token", command, now, cancellationToken);
            return InvalidRefreshToken;
        }

        // ── The order of these two checks is the whole design. ────────────────────────────────────
        //
        // A rotated token is ALSO an unusable token, so a single `if (!session.IsUsable(now))` would
        // reject the replay correctly — and silently. The family would survive, the thief would keep
        // their newer token, and nothing would ever be recorded. The system would look like it was
        // working.
        //
        // Reuse is therefore checked FIRST, and as its own thing.
        if (session.IsRotated)
        {
            await sessions.RevokeFamilyAsync(session.FamilyId, SessionRevocationReason.ReuseDetected, cancellationToken);

            var owner = await repository.FindByIdAsync(session.UserId, cancellationToken);

            await RecordAsync(
                session.UserId,
                owner?.Email,
                LoginEventType.TokenReuseDetected,
                "rotated_token_replayed",
                command,
                now,
                cancellationToken);

            return InvalidRefreshToken;
        }

        if (!session.IsUsable(now))
        {
            var owner = await repository.FindByIdAsync(session.UserId, cancellationToken);

            await RecordAsync(
                session.UserId,
                owner?.Email,
                LoginEventType.TokenRefreshFailed,
                session.IsRevoked ? "session_revoked" : "session_expired",
                command,
                now,
                cancellationToken);

            return InvalidRefreshToken;
        }

        var user = await repository.FindByIdAsync(session.UserId, cancellationToken);

        // The account may have been locked, deactivated or deleted since the session started. A refresh
        // token is not a bypass — it renews an access token, and only for someone still allowed in.
        if (user is null || !user.CanSignIn(now))
        {
            await sessions.RevokeFamilyAsync(session.FamilyId, SessionRevocationReason.AdminRevoked, cancellationToken);

            await RecordAsync(
                session.UserId,
                user?.Email,
                LoginEventType.TokenRefreshFailed,
                "account_unavailable",
                command,
                now,
                cancellationToken);

            return InvalidRefreshToken;
        }

        var issued = sessions.Rotate(session, command.Context);

        // Roles are re-read, not carried over. A role revoked while the session was alive must not
        // survive in the next access token just because the session did.
        var roles = await repository.GetRolesAsync(user.Id, cancellationToken);

        repository.AddLoginEvent(new UserLoginEvent
        {
            Id = Guid.CreateVersion7(),
            UserId = user.Id,
            Email = user.Email,
            EventType = LoginEventType.TokenRefreshed,
            IsSuccessful = true,
            IpAddressHash = command.Context.IpAddressHash,
            UserAgentHash = command.Context.UserAgentHash,
            CreatedAtUtc = now,
        });

        // One save. The old session's ReplacedBySessionId and the new session's row commit together —
        // if they did not, a crash between them would leave a token that is neither rotated nor
        // replaced, and the next use of it would look legitimate forever.
        await repository.SaveChangesAsync(cancellationToken);

        var accessToken = accessTokenIssuer.Issue(user, roles);

        return Result<RefreshResult>.Success(new RefreshResult(
            accessToken.Token,
            accessToken.ExpiresAtUtc,
            issued.RawRefreshToken,
            issued.Session.ExpiresAtUtc,
            user.Id));
    }

    private async Task RecordAsync(
        Guid? userId,
        string? email,
        LoginEventType eventType,
        string reason,
        RefreshCommand command,
        DateTime now,
        CancellationToken cancellationToken)
    {
        repository.AddLoginEvent(new UserLoginEvent
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,

            // The audit log needs a subject even when we cannot name one. It never gets the token.
            Email = email ?? "(unknown)",
            EventType = eventType,
            IsSuccessful = false,
            FailureReason = reason,
            IpAddressHash = command.Context.IpAddressHash,
            UserAgentHash = command.Context.UserAgentHash,
            CreatedAtUtc = now,
        });

        await repository.SaveChangesAsync(cancellationToken);
    }
}
