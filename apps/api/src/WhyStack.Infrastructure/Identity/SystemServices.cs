using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using WhyStack.Application.Abstractions;

namespace WhyStack.Infrastructure.Identity;

public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}

/// <summary>
/// SHA-256, for values that must be comparable but not readable: refresh tokens, reset tokens, and the
/// IP addresses and user agents `07` requires be stored hashed.
/// </summary>
/// <remarks>
/// <b>Not a password hash, and deliberately not.</b> PBKDF2 is slow ON PURPOSE, because a password is
/// short, guessable, and chosen by a human. A refresh token is 256 bits from a CSPRNG: there is
/// nothing to brute-force, so the slowness would buy nothing and would be paid on every single
/// authenticated request. SHA-256 is the right tool precisely because it is fast.
///
/// Using PBKDF2 here would look more secure and be strictly worse. Using SHA-256 for a PASSWORD would
/// look the same and be a catastrophe. Which one is correct depends entirely on whether the input is
/// guessable.
/// </remarks>
public sealed class Sha256TokenHasher : ITokenHasher
{
    public string Hash(string value) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}

/// <summary>
/// 256 bits from the OS cryptographic RNG, URL-safe base64.
/// </summary>
/// <remarks>
/// <b>Not <c>Guid.NewGuid()</c>.</b> A v4 GUID has 122 bits of randomness, six of which are fixed
/// version and variant bits — and, more to the point, "it looks random" is not a security property.
/// <b>Not <c>Random</c>.</b> That is a deterministic sequence seeded from the clock: observe one value
/// and you can compute the next.
///
/// This is a bearer credential. Whoever holds it IS the user, for thirty days. It gets the same RNG a
/// key would.
/// </remarks>
public sealed class CryptoRandomTokenGenerator : ITokenGenerator
{
    private const int TokenBytes = 32;

    public string NewToken() =>
        // Base64Url, not Base64: the token travels in a cookie and a JSON body, and '+' and '/' need
        // escaping in neither if they are never produced.
        Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(TokenBytes));
}
