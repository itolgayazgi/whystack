using WhyStack.Application.Abstractions;
using WhyStack.Application.Common;
using WhyStack.Application.Identity.Tokens;
using WhyStack.Domain.Identity;

namespace WhyStack.Application.Identity.Passwords;

public sealed record ForgotPasswordCommand(string? Email, string? IpAddressHash, string? UserAgentHash);

public sealed record ForgotPasswordResult(string Message);

/// <summary>
/// Sends a reset link — or does nothing at all, and says the same thing either way.
/// </summary>
/// <remarks>
/// This endpoint is the single easiest place to lose account-enumeration protection, because the
/// helpful answer is right there: "no account with that address". Ship that and anyone can feed it a
/// list of email addresses and learn which of them have an account here.
///
/// So the response never varies. It does not vary when the address is unknown, when the account is
/// locked, or when it is deleted. The only thing that varies is whether an email is actually sent —
/// and only the person holding that inbox can observe the difference.
/// </remarks>
public sealed class ForgotPasswordHandler(
    IIdentityRepository repository,
    SingleUseTokenService tokens,
    IEmailSender emailSender,
    IAppLinks links,
    IClock clock)
{
    private const string SameAnswerEitherWay =
        "If that address has an account, we have sent it a link to reset the password.";

    public async Task<Result<ForgotPasswordResult>> HandleAsync(
        ForgotPasswordCommand command,
        CancellationToken cancellationToken)
    {
        // A malformed address is told so. That leaks nothing — it is a fact about the string the caller
        // typed, not about who exists — and withholding it would only punish someone with a typo.
        if (!EmailAddress.LooksValid(command.Email))
        {
            return Error.Validation("email", "A valid email address is required.");
        }

        var normalized = EmailAddress.Normalize(command.Email!);
        var user = await repository.FindByNormalizedEmailAsync(normalized, cancellationToken);
        var now = clock.UtcNow;

        if (user is null)
        {
            // Nothing to do, and nothing to say. Note that we do NOT skip the work loudly — no log at
            // warning level, no metric named "reset_for_unknown_user" that an operator might expose.
            return Result<ForgotPasswordResult>.Success(new ForgotPasswordResult(SameAnswerEitherWay));
        }

        // A locked account may still reset. Refusing here would be backwards: forgetting your password
        // is exactly how you get locked out in the first place, and the reset is the way back in.
        // A DELETED account may not — there is nothing to reset.
        if (user.IsDeleted)
        {
            return Result<ForgotPasswordResult>.Success(new ForgotPasswordResult(SameAnswerEitherWay));
        }

        var token = await tokens.IssuePasswordResetAsync(user, cancellationToken);

        repository.AddLoginEvent(new UserLoginEvent
        {
            Id = Guid.CreateVersion7(),
            UserId = user.Id,
            Email = user.Email,
            EventType = LoginEventType.PasswordResetRequested,
            IsSuccessful = true,
            IpAddressHash = command.IpAddressHash,
            UserAgentHash = command.UserAgentHash,
            CreatedAtUtc = now,
        });

        await repository.SaveChangesAsync(cancellationToken);

        await emailSender.SendAsync(
            new EmailMessage(
                To: user.Email,
                Subject: "Reset your WhyStack password",
                Body: $"""
                    Someone asked to reset the password for this account.

                    {links.ResetPassword(token)}

                    The link works once, and expires in {SingleUseTokenService.PasswordResetLifetime.TotalMinutes:0} minutes.

                    If this was not you, ignore this email. Your password has not changed, and nobody
                    can use this link without opening it from your inbox.
                    """),
            cancellationToken);

        return Result<ForgotPasswordResult>.Success(new ForgotPasswordResult(SameAnswerEitherWay));
    }
}
