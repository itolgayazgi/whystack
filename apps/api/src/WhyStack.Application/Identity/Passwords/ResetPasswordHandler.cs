using WhyStack.Application.Abstractions;
using WhyStack.Application.Common;
using WhyStack.Application.Identity.Sessions;
using WhyStack.Application.Identity.Tokens;
using WhyStack.Domain.Identity;

namespace WhyStack.Application.Identity.Passwords;

public sealed record ResetPasswordCommand(
    string? Token,
    string? NewPassword,
    string? IpAddressHash,
    string? UserAgentHash);

public sealed record ResetPasswordResult(int SessionsEnded);

public sealed class ResetPasswordHandler(
    IIdentityRepository repository,
    SingleUseTokenService tokens,
    SessionService sessions,
    IPasswordHasher passwordHasher,
    IEmailSender emailSender,
    IClock clock)
{
    /// <summary>
    /// Expired, already used, or never issued — all the same answer. Telling the caller "that token was
    /// already used" confirms they are holding a real one, which is precisely the information someone
    /// who found it in a forwarded email should not get.
    /// </summary>
    private static readonly Error InvalidToken = new(
        ErrorCodes.InvalidResetToken,
        "This reset link is no longer valid. Please request a new one.");

    public async Task<Result<ResetPasswordResult>> HandleAsync(
        ResetPasswordCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Token))
        {
            return InvalidToken;
        }

        var now = clock.UtcNow;
        var token = await tokens.FindPasswordResetAsync(command.Token, cancellationToken);

        if (token is null || !token.IsRedeemable(now))
        {
            return InvalidToken;
        }

        var user = await repository.FindByIdAsync(token.UserId, cancellationToken);

        if (user is null || user.IsDeleted)
        {
            return InvalidToken;
        }

        // The password is validated only AFTER the token proves out. Doing it first would let someone
        // with no token at all learn the password rules — harmless — but would also mean a garbage
        // token and a short password produce different errors, which is one more bit of oracle for free.
        var failures = PasswordPolicy.Validate(command.NewPassword, user.Email);
        if (failures.Count > 0)
        {
            return Error.Validation("newPassword", failures[0]);
        }

        tokens.Redeem(token);

        user.PasswordHash = passwordHasher.Hash(command.NewPassword!);
        user.UpdatedAtUtc = now;

        // The reset proves control of the inbox — which is exactly what email confirmation proves.
        // Leaving the address unconfirmed after someone demonstrably read mail sent to it would be
        // pedantry with a cost: the user would still be nagged to confirm.
        user.IsEmailConfirmed = true;

        // Whoever did this could not sign in. Leaving the lockout in place would mean the reset appears
        // to work and the next sign-in still fails, with nothing to explain why.
        user.IsLocked = false;
        user.LockedUntilUtc = null;
        user.FailedLoginAttempts = 0;

        // ── The line that matters most in this file. ─────────────────────────────────────────────
        //
        // Every session dies. Not for tidiness: changing your password is what you DO when you think
        // someone else is in your account, and a reset that leaves their refresh token working means
        // the attacker keeps the account while the victim congratulates themselves on fixing it.
        //
        // The user signs in again on their own devices. That is a small cost, once, and it is the only
        // version of this that actually evicts anybody.
        var active = await repository.GetActiveSessionsAsync(user.Id, cancellationToken);
        var ended = active.Count;
        await sessions.RevokeAllForUserAsync(user.Id, SessionRevocationReason.PasswordChanged, cancellationToken);

        // Outstanding reset tokens die too. Two reset emails in an inbox must not mean two chances.
        await repository.InvalidateOutstandingPasswordResetTokensAsync(user.Id, cancellationToken);

        repository.AddLoginEvent(new UserLoginEvent
        {
            Id = Guid.CreateVersion7(),
            UserId = user.Id,
            Email = user.Email,
            EventType = LoginEventType.PasswordResetCompleted,
            IsSuccessful = true,
            IpAddressHash = command.IpAddressHash,
            UserAgentHash = command.UserAgentHash,
            CreatedAtUtc = now,
        });

        await repository.SaveChangesAsync(cancellationToken);

        // Told, not asked. If this password change was not the user's doing, this email is the only way
        // they find out — and it arrives at an address the attacker does not control, which is what
        // makes it worth sending.
        await emailSender.SendAsync(
            new EmailMessage(
                To: user.Email,
                Subject: "Your WhyStack password was changed",
                Body: """
                    Your password has just been changed, and every device has been signed out.

                    If this was you, there is nothing to do — sign in again with the new password.

                    If it was NOT you, someone has access to this email account. Reset your password
                    again immediately and secure your inbox.
                    """),
            cancellationToken);

        return Result<ResetPasswordResult>.Success(new ResetPasswordResult(ended));
    }
}
