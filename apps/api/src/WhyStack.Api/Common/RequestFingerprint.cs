using WhyStack.Application.Abstractions;

namespace WhyStack.Api.Common;

/// <summary>
/// Hashes the caller's IP address and user agent for the audit log.
/// </summary>
/// <remarks>
/// `07`: "Sensitive metadata should be privacy-reviewed." An IP address is personal data under GDPR,
/// and a user agent is most of a browser fingerprint. We do not need to READ either — we need to know
/// that two events came from the same place, which a hash answers exactly as well.
///
/// So the audit log can still say "these forty failed logins all came from one address", and a breach
/// of that log still tells the attacker nothing about where anybody lives.
/// </remarks>
public static class RequestFingerprint
{
    public static (string? IpHash, string? UserAgentHash) Of(HttpContext context, ITokenHasher hasher)
    {
        var ip = context.Connection.RemoteIpAddress?.ToString();
        var userAgent = context.Request.Headers.UserAgent.ToString();

        return (
            string.IsNullOrWhiteSpace(ip) ? null : hasher.Hash(ip),
            string.IsNullOrWhiteSpace(userAgent) ? null : hasher.Hash(userAgent));
    }
}
