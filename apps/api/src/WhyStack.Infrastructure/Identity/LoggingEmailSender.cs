using Microsoft.Extensions.Logging;
using WhyStack.Application.Abstractions;

namespace WhyStack.Infrastructure.Identity;

/// <summary>
/// Writes the mail to the log instead of sending it. <b>Development only.</b>
/// </summary>
/// <remarks>
/// This is a stand-in, and it says so out loud on every call rather than pretending to work. A silent
/// no-op sender is worse than none: registration would look healthy while nobody ever received a
/// confirmation, and the first person to notice would be a user who could not sign in.
///
/// The composition root refuses to register this outside Development — see AddInfrastructure. An
/// environment with no mail configured must fail to start, not start and quietly deliver nothing.
/// </remarks>
public sealed partial class LoggingEmailSender(ILogger<LoggingEmailSender> logger) : IEmailSender
{
    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        NotSent(logger, message.To, message.Subject, message.Body);
        return Task.CompletedTask;
    }

    [LoggerMessage(
        EventId = 1000,
        Level = LogLevel.Warning,
        Message = "EMAIL NOT SENT (development stand-in). To: {To} | Subject: {Subject}\n{Body}")]
    private static partial void NotSent(ILogger logger, string to, string subject, string body);
}
