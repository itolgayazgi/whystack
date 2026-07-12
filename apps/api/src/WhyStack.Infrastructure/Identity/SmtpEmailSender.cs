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
    /// Off only for a local mail catcher, which speaks plain SMTP on a loopback port and has no
    /// certificate. Anywhere else, a reset link crossing the network in the clear is a reset link
    /// somebody else now has.
    /// </summary>
    public bool UseTls { get; init; } = true;
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
        var mail = new MimeMessage
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
            _options.UseTls ? SecureSocketOptions.StartTlsWhenAvailable : SecureSocketOptions.None,
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
