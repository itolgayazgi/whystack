using WhyStack.Application.Abstractions;
using WhyStack.Application.Common;
using WhyStack.Application.Identity.Tokens;
using WhyStack.Domain.Identity;

namespace WhyStack.Application.Identity.Confirmation;

public sealed record ConfirmEmailCommand(string? Token, string? IpAddressHash, string? UserAgentHash);

public sealed record ConfirmEmailResult(string Email);

public sealed class ConfirmEmailHandler(
    IIdentityRepository repository,
    SingleUseTokenService tokens,
    IClock clock)
{
    private static readonly Error InvalidToken = new(
        ErrorCodes.InvalidConfirmationToken,
        "This confirmation link is no longer valid. Please request a new one.");

    public async Task<Result<ConfirmEmailResult>> HandleAsync(
        ConfirmEmailCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Token))
        {
            return InvalidToken;
        }

        var now = clock.UtcNow;
        var token = await tokens.FindEmailConfirmationAsync(command.Token, cancellationToken);

        if (token is null || !token.IsRedeemable(now))
        {
            return InvalidToken;
        }

        var user = await repository.FindByIdAsync(token.UserId, cancellationToken);

        if (user is null || user.IsDeleted)
        {
            return InvalidToken;
        }

        // The token confirms the address it was SENT to, not whatever the account says now.
        //
        // Otherwise: register as ada@x.com, get the link, change the account's email to
        // victim@bank.com, then click. The account would end up with a "confirmed" address that nobody
        // at that address ever agreed to — and confirmation would prove nothing at all.
        if (!string.Equals(token.Email, user.Email, StringComparison.OrdinalIgnoreCase))
        {
            return InvalidToken;
        }

        tokens.Redeem(token);

        user.IsEmailConfirmed = true;
        user.UpdatedAtUtc = now;

        repository.AddLoginEvent(new UserLoginEvent
        {
            Id = Guid.CreateVersion7(),
            UserId = user.Id,
            Email = user.Email,
            EventType = LoginEventType.EmailConfirmed,
            IsSuccessful = true,
            IpAddressHash = command.IpAddressHash,
            UserAgentHash = command.UserAgentHash,
            CreatedAtUtc = now,
        });

        await repository.SaveChangesAsync(cancellationToken);

        return Result<ConfirmEmailResult>.Success(new ConfirmEmailResult(user.Email));
    }
}

public sealed record ResendConfirmationCommand(string? Email);

public sealed record ResendConfirmationResult(string Message);

/// <summary>
/// Re-sends the confirmation link. Enumeration-safe, for the same reason forgot-password is.
/// </summary>
public sealed class ResendConfirmationHandler(
    IIdentityRepository repository,
    SingleUseTokenService tokens,
    IEmailSender emailSender,
    IAppLinks links)
{
    private const string SameAnswerEitherWay =
        "If that address has an unconfirmed account, we have sent it a new confirmation link.";

    public async Task<Result<ResendConfirmationResult>> HandleAsync(
        ResendConfirmationCommand command,
        CancellationToken cancellationToken)
    {
        if (!EmailAddress.LooksValid(command.Email))
        {
            return Error.Validation("email", "A valid email address is required.");
        }

        var user = await repository.FindByNormalizedEmailAsync(
            EmailAddress.Normalize(command.Email!),
            cancellationToken);

        // Unknown address, deleted account, or an address that is ALREADY confirmed — all silent.
        // "That address is already confirmed" would be a perfectly friendly way to tell an attacker
        // both that the account exists and that it is in use.
        if (user is null || user.IsDeleted || user.IsEmailConfirmed)
        {
            return Result<ResendConfirmationResult>.Success(new ResendConfirmationResult(SameAnswerEitherWay));
        }

        var token = await tokens.IssueEmailConfirmationAsync(user, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        await emailSender.SendAsync(
            new EmailMessage(
                To: user.Email,
                Subject: "Confirm your WhyStack account",
                Body: $"""
                    Confirm this address to finish setting up your account.

                    {links.ConfirmEmail(token)}

                    The link works once, and expires in {SingleUseTokenService.EmailConfirmationLifetime.TotalHours:0} hours.

                    If you did not create a WhyStack account, ignore this email.
                    """),
            cancellationToken);

        return Result<ResendConfirmationResult>.Success(new ResendConfirmationResult(SameAnswerEitherWay));
    }
}
