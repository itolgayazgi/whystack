using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace WhyStack.Api.Common;

/// <summary>
/// Teaches the OpenAPI document that this API is protected by a bearer token.
/// </summary>
/// <remarks>
/// Without this the document lists the endpoints and says nothing about authentication, so the API explorer
/// has no field to put a token in — and every authorised call it makes comes back 401. Which looks like a
/// broken API rather than a missing header, and is exactly the kind of false failure that sends somebody
/// debugging the wrong thing.
///
/// The scheme is only DESCRIBED here. It is enforced by the authorization policies on the endpoints; a
/// document transformer cannot protect anything, and must never be mistaken for a thing that does.
/// </remarks>
public sealed class BearerSecurityTransformer(IAuthenticationSchemeProvider schemes)
    : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        var registered = await schemes.GetAllSchemesAsync();

        if (!registered.Any(scheme => scheme.Name == "Bearer"))
        {
            return;
        }

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description =
                "The 15-minute access token from POST /api/v1/auth/login. Paste the token itself — the "
                + "explorer adds the 'Bearer ' prefix. The refresh token is NOT here: on the web it lives in "
                + "an HttpOnly cookie the browser sends by itself, and JavaScript is not allowed to read it "
                + "(ADR-0008).",
        };
    }
}
