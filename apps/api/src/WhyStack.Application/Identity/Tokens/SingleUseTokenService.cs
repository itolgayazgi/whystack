using WhyStack.Application.Abstractions;
using WhyStack.Domain.Identity;

namespace WhyStack.Application.Identity.Tokens;

/// <summary>
/// Issues and redeems the email-confirmation and password-reset tokens. One place, because both get
/// the same three things wrong in the same three ways when they are written twice.
/// </summary>
public sealed class SingleUseTokenService(
    IIdentityRepository repository,
    ITokenGenerator tokenGenerator,
    ITokenHasher tokenHasher,
    IClock clock)
{
    /// <summary>
    /// One hour. A password-reset token IS a password — it lets whoever holds it take the account.
    /// The only thing making it safer than a password is that it dies quickly, so the window is the
    /// whole security property. Long enough for a slow mail server; far too short to sit in a mailbox
    /// that gets breached next month.
    /// </summary>
    public static readonly TimeSpan PasswordResetLifetime = TimeSpan.FromHours(1);

    /// <summary>
    /// A day. Longer than a reset token because it is worth far less — it proves you read your mail,
    /// it does not let you sign in — and because "confirm your account" mail is routinely read the
    /// next morning. Expiring it in an hour would just teach people that the link never works.
    /// </summary>
    public static readonly TimeSpan EmailConfirmationLifetime = TimeSpan.FromDays(1);

    public async Task<string> IssueEmailConfirmationAsync(
        User user,
        CancellationToken cancellationToken)
    {
        await repository.InvalidateOutstandingEmailConfirmationTokensAsync(user.Id, cancellationToken);

        var raw = tokenGenerator.NewToken();
        var now = clock.UtcNow;

        repository.AddEmailConfirmationToken(new EmailConfirmationToken
        {
            Id = Guid.CreateVersion7(),
            UserId = user.Id,

            // Hashed. The raw token exists in exactly one place: the email. If this table leaks, nobody
            // can confirm anybody's address with what they find in it.
            TokenHash = tokenHasher.Hash(raw),

            // The address this token confirms, captured now. A user may change their email before
            // clicking, and a token must confirm the address it was SENT to — not whatever the row says
            // by the time they get around to it.
            Email = user.Email,

            CreatedAtUtc = now,
            ExpiresAtUtc = now.Add(EmailConfirmationLifetime),
        });

        return raw;
    }

    public async Task<string> IssuePasswordResetAsync(User user, CancellationToken cancellationToken)
    {
        await repository.InvalidateOutstandingPasswordResetTokensAsync(user.Id, cancellationToken);

        var raw = tokenGenerator.NewToken();
        var now = clock.UtcNow;

        repository.AddPasswordResetToken(new PasswordResetToken
        {
            Id = Guid.CreateVersion7(),
            UserId = user.Id,
            TokenHash = tokenHasher.Hash(raw),
            CreatedAtUtc = now,
            ExpiresAtUtc = now.Add(PasswordResetLifetime),
        });

        return raw;
    }

    public Task<EmailConfirmationToken?> FindEmailConfirmationAsync(string raw, CancellationToken cancellationToken) =>
        repository.FindEmailConfirmationTokenAsync(tokenHasher.Hash(raw), cancellationToken);

    public Task<PasswordResetToken?> FindPasswordResetAsync(string raw, CancellationToken cancellationToken) =>
        repository.FindPasswordResetTokenAsync(tokenHasher.Hash(raw), cancellationToken);

    /// <summary>Spends a token. Called once, and only once — that is the entire point of the type.</summary>
    public void Redeem(SingleUseToken token) => token.UsedAtUtc = clock.UtcNow;
}
