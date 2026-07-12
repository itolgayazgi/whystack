using WhyStack.Application.Abstractions;
using WhyStack.Domain.Identity;

namespace WhyStack.Application.Identity.Sessions;

/// <summary>Where the refresh token is born, rotated and killed. One place, so the rules cannot diverge.</summary>
public sealed record IssuedSession(UserSession Session, string RawRefreshToken);

public sealed record SessionContext(
    string? Platform,
    string? DeviceType,
    string? IpAddressHash,
    string? UserAgentHash);

public sealed class SessionService(
    IIdentityRepository repository,
    ITokenGenerator tokenGenerator,
    ITokenHasher tokenHasher,
    IClock clock)
{
    /// <summary>
    /// Thirty days, refreshed on every rotation. There is deliberately NO absolute cap yet — a session
    /// that stays active can live indefinitely. That is a real gap, it is recorded as an issue, and it
    /// is survivable only because reuse detection means a STOLEN token cannot quietly live that long:
    /// the moment the legitimate client next refreshes, the thief's chain is detected and killed.
    /// </summary>
    public static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(30);

    /// <summary>Starts a new family. This is a sign-in, not a rotation.</summary>
    public IssuedSession StartSession(Guid userId, SessionContext context)
    {
        var familyId = Guid.CreateVersion7();
        return CreateSession(userId, familyId, context);
    }

    /// <summary>
    /// Exchanges a session for its successor: same family, new token, and the old one marked as
    /// <see cref="UserSession.ReplacedBySessionId"/> — which is what makes a later use of it a REPLAY
    /// rather than an ordinary failure.
    /// </summary>
    public IssuedSession Rotate(UserSession current, SessionContext context)
    {
        var issued = CreateSession(current.UserId, current.FamilyId, context);

        current.ReplacedBySessionId = issued.Session.Id;
        current.LastUsedAtUtc = clock.UtcNow;

        // RevokedAtUtc stays null on purpose. This token was not revoked — it was superseded, and the
        // difference matters: a revoked token is one we killed, a rotated one is evidence. Marking it
        // revoked would blur the two and make "was this replayed, or just logged out?" unanswerable.
        return issued;
    }

    /// <summary>
    /// The response to a replay: kill the entire chain from that sign-in.
    ///
    /// Not just the replayed token. If a token leaked, the attacker may already hold a NEWER one in the
    /// same family — rejecting only the old one leaves them signed in while the victim is not, which is
    /// the exact opposite of what the defence is for.
    /// </summary>
    public async Task RevokeFamilyAsync(
        Guid familyId,
        SessionRevocationReason reason,
        CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;

        foreach (var session in await repository.GetFamilyAsync(familyId, cancellationToken))
        {
            if (session.RevokedAtUtc is null)
            {
                session.RevokedAtUtc = now;
                session.RevocationReason = reason;
            }
        }
    }

    public async Task RevokeAllForUserAsync(
        Guid userId,
        SessionRevocationReason reason,
        CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;

        foreach (var session in await repository.GetActiveSessionsAsync(userId, cancellationToken))
        {
            session.RevokedAtUtc = now;
            session.RevocationReason = reason;
        }
    }

    public Task<UserSession?> FindByRawTokenAsync(string rawToken, CancellationToken cancellationToken) =>
        repository.FindSessionByRefreshTokenHashAsync(tokenHasher.Hash(rawToken), cancellationToken);

    private IssuedSession CreateSession(Guid userId, Guid familyId, SessionContext context)
    {
        var rawToken = tokenGenerator.NewToken();
        var now = clock.UtcNow;

        var session = new UserSession
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            FamilyId = familyId,

            // The hash is stored; the raw token is returned to the client once and never written down.
            // A database dump therefore hands nobody a session (ADR-0008, `07`, `12`).
            RefreshTokenHash = tokenHasher.Hash(rawToken),

            Platform = context.Platform,
            DeviceType = context.DeviceType,
            IpAddressHash = context.IpAddressHash,
            UserAgentHash = context.UserAgentHash,
            CreatedAtUtc = now,
            ExpiresAtUtc = now.Add(RefreshTokenLifetime),
        };

        repository.AddSession(session);

        return new IssuedSession(session, rawToken);
    }
}
