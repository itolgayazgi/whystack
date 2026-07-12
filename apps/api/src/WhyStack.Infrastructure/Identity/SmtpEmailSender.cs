using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using WhyStack.Application.Abstractions;

namespace WhyStack.Infrastructure.Identity;

public sealed class SmtpOptions
{
    public const string SectionName = "Smtp";

    public required string Host { get; init; }

    public int Port { get; init; } = 587;

    public string? Username { get; init; }

    /// <summary>Never in appsettings. User secrets locally; a secret store in production.</summary>
    public string? Password { get; init; }

    public required string FromAddress { get; init; }

    public string FromName { get; init; } = "WhyStack";

    /// <summary>
    /// How the connection is secured. Defaults to the strict option, so a missing setting fails closed.
    /// </summary>
    public SmtpSecurity Security { get; init; } = SmtpSecurity.StartTls;
}

/// <summary>
/// The three ways to reach an SMTP server, and the reason this is not a <c>bool</c>.
/// </summary>
/// <remarks>
/// <b>It used to be <c>UseTls</c>, mapped to MailKit's <c>StartTlsWhenAvailable</c>, and that was a
/// downgrade attack waiting to happen.</b> "When available" means: ask the server whether it supports
/// TLS, and if it says no — or if somebody sitting between us strips the STARTTLS advertisement from
/// its greeting, which is a well-known and trivial attack — send the message in the clear anyway,
/// silently, with no error.
///
/// The body of these messages is a password-reset link. A reset link IS the password: whoever reads it
/// owns the account. Sending one in plaintext because a hostile network said "no TLS here" is the exact
/// silent fallback CLAUDE.md §1.6 forbids, and it would never have appeared in any log.
///
/// <see cref="StartTls"/> REQUIRES the upgrade and fails the send if it cannot get it. A failed email is
/// a visible, fixable problem. A plaintext email is not a problem anyone finds out about.
/// </remarks>
public enum SmtpSecurity
{
    /// <summary>
    /// Plaintext. Legitimate for exactly one thing: a local mail catcher on the loopback interface,
    /// which has no certificate and where there is no network to eavesdrop on. Never for a real server.
    /// </summary>
    None = 0,

    /// <summary>Connect in the clear, then REQUIRE an upgrade to TLS. The usual choice, on port 587.</summary>
    StartTls = 1,

    /// <summary>TLS from the first byte — no plaintext phase at all, so nothing to strip. Port 465.</summary>
    SslOnConnect = 2,
}

/// <summary>
/// SMTP, via MailKit. Provider-agnostic on purpose (ADR-0012): Resend, SendGrid, Postmark, Mailgun and
/// SES all speak SMTP, so the provider is a deployment decision rather than a code one.
/// </summary>
/// <remarks>
/// MailKit rather than <c>System.Net.Mail.SmtpClient</c>, which Microsoft marks obsolete for new
/// development and which does not implement modern TLS negotiation properly.
///
/// <b>Sending is synchronous with the request, and that is a known limitation.</b> A slow or dead mail
/// server makes registration slow or fails it. The right answer is an outbox — write the message in the
/// same transaction as the user, deliver it from a background worker, retry — and that is a queue, a
/// table and a worker this sprint does not have. What it does NOT do is swallow the failure: a send
/// that fails throws, and the caller sees it, because a registration that silently sends nothing is a
/// user who can never confirm and never knows why.
/// </remarks>
public sealed partial class SmtpEmailSender(
    IOptions<SmtpOptions> options,
    ILogger<SmtpEmailSender> logger) : IEmailSender
{
    private readonly SmtpOptions _options = options.Value;

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        // MimeMessage is disposable, and it is not ceremony: it owns the streams behind its body parts.
        // Leaving it to the finaliser means those streams live until a GC that may not come under load,
        // which is exactly when you are sending the most mail.
        using var mail = new MimeMessage
        {
            Subject = message.Subject,
            Body = new TextPart("plain") { Text = message.Body },
        };

        mail.From.Add(new MailboxAddress(_options.FromName, _options.FromAddress));
        mail.To.Add(MailboxAddress.Parse(message.To));

        using var client = new SmtpClient();

        await client.ConnectAsync(
            _options.Host,
            _options.Port,
            _options.Security switch
            {
                SmtpSecurity.None => SecureSocketOptions.None,

                // StartTls, and NOT StartTlsWhenAvailable. The difference is the whole point: "when
                // available" hands the decision to whoever is on the other end of the wire, including
                // an attacker who has stripped the STARTTLS advertisement — and then sends a
                // password-reset link in plaintext without a word in any log.
                SmtpSecurity.StartTls => SecureSocketOptions.StartTls,

                SmtpSecurity.SslOnConnect => SecureSocketOptions.SslOnConnect,

                _ => throw new InvalidOperationException($"Unknown SMTP security mode: {_options.Security}."),
            },
            cancellationToken);

        // Both, or neither. A local mail catcher wants no credentials at all; every hosted provider
        // wants both. A username with no password is a misconfiguration that would otherwise surface as
        // a NullReferenceException inside MailKit.
        if (!string.IsNullOrWhiteSpace(_options.Username) && !string.IsNullOrWhiteSpace(_options.Password))
        {
            await client.AuthenticateAsync(_options.Username, _options.Password, cancellationToken);
        }

        await client.SendAsync(mail, cancellationToken);
        await client.DisconnectAsync(quit: true, cancellationToken);

        // The subject, never the body. The body of these emails contains a reset link, and a reset link
        // is a password — putting it in the log puts it in every log aggregator, forever
        // (`12` logging rules).
        Sent(logger, message.To, message.Subject);
    }

    [LoggerMessage(EventId = 1001, Level = LogLevel.Information, Message = "Email sent to {To}: {Subject}")]
    private static partial void Sent(ILogger logger, string to, string subject);
}
