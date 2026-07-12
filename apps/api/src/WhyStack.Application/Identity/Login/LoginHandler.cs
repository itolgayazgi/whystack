using WhyStack.Application.Abstractions;
using WhyStack.Application.Common;
using WhyStack.Domain.Identity;

namespace WhyStack.Application.Identity.Login;

/// <summary>Email and Password are nullable because JSON can deliver null for either.</summary>
public sealed record LoginCommand(
    string? Email,
    string? Password,
    string? IpAddressHash,
    string? UserAgentHash);

public sealed record LoginResult(
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    Guid UserId,
    string Email,
    string? DisplayName,
    bool IsEmailConfirmed,
    IReadOnlyCollection<string> Roles);

/// <summary>
/// Sign-in. Every failure looks the same from the outside, and takes about the same time.
/// </summary>
public sealed class LoginHandler(
    IIdentityRepository repository,
    IPasswordHasher passwordHasher,
    IAccessTokenIssuer accessTokenIssuer,
    IClock clock)
{
    /// <summary>
    /// Five is enough to catch a human who has forgotten which password they used, and far too few for
    /// a machine to make progress.
    /// </summary>
    public const int MaximumFailedAttempts = 5;

    /// <summary>
    /// Fifteen minutes, and then it lifts by itself. A lock that needs an administrator to clear it is
    /// a denial of service anyone can trigger against anyone, for free.
    /// </summary>
    public static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    /// <summary>
    /// A real PasswordHasher output for a password nobody has. It exists to be verified against when
    /// the account does NOT exist — see the timing note in <see cref="HandleAsync"/>.
    /// </summary>
    private const string DecoyHash =
        "AQAAAAIAAYagAAAAEK9Cm5tvVYtNAY6Vlqe6vPn7hUpKLzhnfYlHt9NJz1jXqO3NwzZ0k8sZ1D8xVXfXAg==";

    private static readonly Error InvalidCredentials = new(
        ErrorCodes.InvalidCredentials,
        "The email address or password is incorrect.");

    public async Task<Result<LoginResult>> HandleAsync(
        LoginCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Email) || string.IsNullOrWhiteSpace(command.Password))
        {
            return InvalidCredentials;
        }

        var now = clock.UtcNow;
        var normalizedEmail = EmailAddress.Normalize(command.Email!);
        var user = await repository.FindByNormalizedEmailAsync(normalizedEmail, cancellationToken);

        // The timing oracle, and why the decoy exists.
        //
        // The obvious code returns here when the user is null. That reply comes back in a millisecond,
        // while a real account takes the ~100ms that password hashing is DESIGNED to take. The
        // difference is trivially measurable over a few requests — so "invalid credentials" arriving
        // fast means "no such account", and the identical error message has leaked exactly what it was
        // written to hide.
        //
        // So we verify the given password against a decoy hash instead: same work, same time, same
        // answer. This is the whole reason enumeration protection is more than a wording exercise.
        if (user is null)
        {
            passwordHasher.Verify(DecoyHash, command.Password!);

            await RecordFailureAsync(
                userId: null,
                email: EmailAddress.Format(command.Email!),
                reason: "unknown_account",
                command,
                now,
                cancellationToken);

            return InvalidCredentials;
        }

        // Locked, deactivated or deleted — all indistinguishable from a wrong password, and checked
        // BEFORE the password. Answering "your account is locked" to someone who does not know the
        // password would confirm the account exists.
        if (!user.CanSignIn(now))
        {
            passwordHasher.Verify(user.PasswordHash, command.Password!);

            await RecordFailureAsync(
                user.Id,
                user.Email,
                user.IsLockedAt(now) ? "account_locked" : "account_unavailable",
                command,
                now,
                cancellationToken);

            return InvalidCredentials;
        }

        var verification = passwordHasher.Verify(user.PasswordHash, command.Password!);

        if (verification == PasswordVerificationResult.Failed)
        {
            user.FailedLoginAttempts++;
            user.UpdatedAtUtc = now;

            var justLocked = user.FailedLoginAttempts >= MaximumFailedAttempts;
            if (justLocked)
            {
                user.IsLocked = true;
                user.LockedUntilUtc = now.Add(LockoutDuration);
            }

            await RecordFailureAsync(
                user.Id,
                user.Email,
                justLocked ? "locked_out" : "wrong_password",
                command,
                now,
                cancellationToken,
                extraEvent: justLocked ? LoginEventType.AccountLocked : null);

            return InvalidCredentials;
        }

        // Correct password, but the stored hash was produced with weaker parameters than we use now.
        // This is the only moment we will ever hold the plaintext again, so it is the only moment the
        // hash can be upgraded. Skip it and the account keeps its weak hash for life.
        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = passwordHasher.Hash(command.Password!);
        }

        user.FailedLoginAttempts = 0;
        user.IsLocked = false;
        user.LockedUntilUtc = null;
        user.LastLoginAtUtc = now;
        user.UpdatedAtUtc = now;

        var roles = await repository.GetRolesAsync(user.Id, cancellationToken);

        repository.AddLoginEvent(new UserLoginEvent
        {
            Id = Guid.CreateVersion7(),
            UserId = user.Id,
            Email = user.Email,
            EventType = LoginEventType.LoginSucceeded,
            IsSuccessful = true,
            IpAddressHash = command.IpAddressHash,
            UserAgentHash = command.UserAgentHash,
            CreatedAtUtc = now,
        });

        await repository.SaveChangesAsync(cancellationToken);

        var accessToken = accessTokenIssuer.Issue(user, roles);

        // An unconfirmed account may sign in — it just cannot do everything. Blocking sign-in until
        // confirmation means a user whose confirmation mail went to spam is locked out of the very
        // screen that would let them ask for another one.
        return Result<LoginResult>.Success(new LoginResult(
            accessToken.Token,
            accessToken.ExpiresAtUtc,
            user.Id,
            user.Email,
            user.DisplayName,
            user.IsEmailConfirmed,
            [.. roles.Select(role => role.ToString())]));
    }

    private async Task RecordFailureAsync(
        Guid? userId,
        string email,
        string reason,
        LoginCommand command,
        DateTime now,
        CancellationToken cancellationToken,
        LoginEventType? extraEvent = null)
    {
        // The reason is written to the AUDIT LOG, never to the response. The operator needs to tell a
        // brute force from a typo; the caller must not be able to tell them apart at all.
        repository.AddLoginEvent(new UserLoginEvent
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            Email = email,
            EventType = LoginEventType.LoginFailed,
            IsSuccessful = false,
            FailureReason = reason,
            IpAddressHash = command.IpAddressHash,
            UserAgentHash = command.UserAgentHash,
            CreatedAtUtc = now,
        });

        if (extraEvent is not null)
        {
            repository.AddLoginEvent(new UserLoginEvent
            {
                Id = Guid.CreateVersion7(),
                UserId = userId,
                Email = email,
                EventType = extraEvent.Value,
                IsSuccessful = false,
                FailureReason = reason,
                IpAddressHash = command.IpAddressHash,
                UserAgentHash = command.UserAgentHash,
                CreatedAtUtc = now,
            });
        }

        await repository.SaveChangesAsync(cancellationToken);
    }
}
