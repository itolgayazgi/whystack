using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace WhyStack.Api.Common;

/// <summary>
/// The id of the authenticated caller, taken from the token and from nowhere else.
/// </summary>
/// <remarks>
/// <b>This is the ownership boundary, and it is one line long.</b> Every authenticated endpoint acts on
/// "me", and "me" is whoever the signed token says it is — never a user id from the route, the query
/// string or the body. That is not a style preference: <c>PUT /users/{id}/preferences</c> with the id
/// taken from the URL is the single most common authorization bug there is (IDOR), and the only reliable
/// defence is to never give the caller a way to name someone else. `08`'s <c>/users/me</c> shape exists
/// for exactly this reason, and this method is what makes it true rather than merely stated.
///
/// It throws rather than returning null. Reaching here without a valid <c>sub</c> claim means the
/// endpoint was mapped without <c>RequireAuthorization()</c> or the token was issued without a subject —
/// both are bugs in OUR code, not bad input, and a 500 with a loud message is the correct answer to
/// both. Returning <c>Guid.Empty</c> instead would hand every caller the same "user", which is a
/// data-leak-shaped hole (CLAUDE.md §1.6 — never a silent fallback).
/// </remarks>
public static class CurrentUser
{
    public static Guid Id(this ClaimsPrincipal principal)
    {
        // JwtSecurityTokenHandler rewrites "sub" into ClaimTypes.NameIdentifier by default, so the claim
        // arrives under a different name than the issuer wrote. Both are checked because which one you
        // get depends on inbound claim mapping — a setting somebody may reasonably change later, and if
        // they do, this must keep working rather than start throwing on every authenticated request.
        var subject =
            principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(subject, out var userId))
        {
            throw new InvalidOperationException(
                "The authenticated principal has no usable 'sub' claim. Either this endpoint is missing "
                + "RequireAuthorization(), or the access token was issued without a subject.");
        }

        return userId;
    }
}
