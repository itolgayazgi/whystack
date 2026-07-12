using WhyStack.Application.Abstractions;
using WhyStack.Application.Common;
using WhyStack.Application.Identity.Sessions;
using WhyStack.Domain.Identity;

namespace WhyStack.Application.Identity.Logout;

public sealed record LogoutCommand(string? RefreshToken, bool AllDevices, SessionContext Context);

public sealed record LogoutResult(int SessionsEnded);

/// <summary>
/// Ends a session — and, unlike every other failure in this domain, succeeds even when it finds nothing.
/// </summary>
/// <remarks>
/// Logging out is the one operation where "it did not work" must never be a visible outcome. A client
/// that is told its logout failed has no useful response: it cannot un-know the token, it cannot try
/// harder, and a user staring at "logout failed" reasonably concludes they are still signed in.
///
/// So an unknown, expired or already-revoked token is a SUCCESS. The end state the caller asked for —
/// "this session is over" — is true in every one of those cases.
/// </remarks>
public sealed class LogoutHandler(
    IIdentityRepository repository,
    SessionService sessions,
    IClock clock)
{
    public async Task<Result<LogoutResult>> HandleAsync(
        LogoutCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.RefreshToken))
        {
            return Result<LogoutResult>.Success(new LogoutResult(0));
        }

        var now = clock.UtcNow;
        var session = await sessions.FindByRawTokenAsync(command.RefreshToken, cancellationToken);

        if (session is null)
        {
            return Result<LogoutResult>.Success(new LogoutResult(0));
        }

        var user = await repository.FindByIdAsync(session.UserId, cancellationToken);
        var reason = command.AllDevices
            ? SessionRevocationReason.LogoutAllDevices
            : SessionRevocationReason.Logout;

        int ended;

        if (command.AllDevices)
        {
            var active = await repository.GetActiveSessionsAsync(session.UserId, cancellationToken);
            ended = active.Count;
            await sessions.RevokeAllForUserAsync(session.UserId, reason, cancellationToken);
        }
        else
        {
            // The whole family, not just this token. The family IS the sign-in on this device — the
            // chain of rotations that descended from it — and leaving its ancestors alive would leave
            // a token behind that still refreshes.
            var family = await repository.GetFamilyAsync(session.FamilyId, cancellationToken);
            ended = family.Count(candidate => candidate.RevokedAtUtc is null);
            await sessions.RevokeFamilyAsync(session.FamilyId, reason, cancellationToken);
        }

        repository.AddLoginEvent(new UserLoginEvent
        {
            Id = Guid.CreateVersion7(),
            UserId = session.UserId,
            Email = user?.Email ?? "(unknown)",
            EventType = LoginEventType.Logout,
            IsSuccessful = true,
            FailureReason = command.AllDevices ? "all_devices" : null,
            IpAddressHash = command.Context.IpAddressHash,
            UserAgentHash = command.Context.UserAgentHash,
            CreatedAtUtc = now,
        });

        await repository.SaveChangesAsync(cancellationToken);

        return Result<LogoutResult>.Success(new LogoutResult(ended));
    }
}
