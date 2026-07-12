using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WhyStack.Application.Abstractions;
using WhyStack.Domain.Identity;

namespace WhyStack.Infrastructure.Identity;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public required string Issuer { get; init; }

    public required string Audience { get; init; }

    /// <summary>
    /// Never in appsettings. User secrets locally, a secret store in production. A signing key in a
    /// tracked file is a signing key in the git history forever, and anyone holding it can mint a
    /// token for any user, including an Administrator.
    /// </summary>
    public required string SigningKey { get; init; }

    /// <summary>
    /// ADR-0008: short, target ~15 minutes.
    ///
    /// The number is a trade. An access token cannot be revoked — that is what "stateless" means, and
    /// checking a revocation list on every request would undo the entire point of it. So its lifetime
    /// IS the revocation delay: sign a user out, and their access token still works until it expires.
    /// Fifteen minutes is a window we accept; a day is not.
    /// </summary>
    public int AccessTokenMinutes { get; init; } = 15;
}

public sealed class JwtAccessTokenIssuer(IOptions<JwtOptions> options, IClock clock) : IAccessTokenIssuer
{
    private readonly JwtOptions _options = options.Value;

    public AccessToken Issue(User user, IReadOnlyCollection<RoleName> roles)
    {
        var now = clock.UtcNow;
        var expiresAt = now.AddMinutes(_options.AccessTokenMinutes);

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey)),
            SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),

            // A unique id per token. It is what will let a specific token be denied later without
            // inventing an identifier for it after the fact.
            new(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString()),

            new(JwtRegisteredClaimNames.Email, user.Email),
            new("email_confirmed", user.IsEmailConfirmed ? "true" : "false"),
        };

        // Roles are baked into the token, and that has a consequence worth knowing: a role revoked
        // right now stays effective until this token expires. That is the same fifteen-minute window
        // as everything else stateless, and it is the price of not hitting the database on every
        // request. Anything that must take effect instantly cannot rely on a claim.
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role.ToString())));

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now,
            expires: expiresAt,
            signingCredentials: credentials);

        return new AccessToken(new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
