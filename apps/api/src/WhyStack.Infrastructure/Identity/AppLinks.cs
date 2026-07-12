using Microsoft.Extensions.Options;
using WhyStack.Application.Abstractions;

namespace WhyStack.Infrastructure.Identity;

public sealed class AppOptions
{
    public const string SectionName = "App";

    /// <summary>
    /// Where the CLIENT lives, not the API. The link in an email is clicked by a human in a browser,
    /// and it must land on a screen — not on a JSON endpoint.
    /// </summary>
    public required string ClientBaseUrl { get; init; }
}

public sealed class AppLinks(IOptions<AppOptions> options) : IAppLinks
{
    private readonly Uri _base = new(options.Value.ClientBaseUrl.TrimEnd('/') + "/");

    // The token goes in the query string, and that is a compromise worth naming.
    //
    // Query strings end up in browser history, in Referer headers, and in the access logs of anything
    // in between. For a token that is single-use and dies in an hour, that is acceptable — by the time
    // it surfaces in a log it is already spent. It would NOT be acceptable for a long-lived credential,
    // and it is the reason these tokens expire as fast as they do.
    //
    // The alternative — POSTing the token from a form — cannot survive being pasted into a browser from
    // an email, which is what people actually do.

    public string ConfirmEmail(string token) =>
        new Uri(_base, $"auth/confirm-email?token={Uri.EscapeDataString(token)}").ToString();

    public string ResetPassword(string token) =>
        new Uri(_base, $"auth/reset-password?token={Uri.EscapeDataString(token)}").ToString();
}
