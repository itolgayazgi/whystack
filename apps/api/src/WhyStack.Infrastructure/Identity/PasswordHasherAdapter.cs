using Microsoft.AspNetCore.Identity;
using WhyStack.Application.Abstractions;
using WhyStack.Domain.Identity;
using ApplicationResult = WhyStack.Application.Abstractions.PasswordVerificationResult;
using MicrosoftResult = Microsoft.AspNetCore.Identity.PasswordVerificationResult;

namespace WhyStack.Infrastructure.Identity;

/// <summary>
/// The one place <c>Microsoft.AspNetCore.Identity</c> is referenced (ADR-0017), and the reason it is
/// referenced at all: <b>we do not write password hashing.</b>
/// </summary>
/// <remarks>
/// <see cref="PasswordHasher{TUser}"/> is PBKDF2-HMAC-SHA512, 210,000 iterations, a per-password salt,
/// and a version byte in the output so the parameters can be raised later without invalidating every
/// existing hash. It also compares in constant time.
///
/// Every one of those is a thing people get wrong when they roll their own, and none of them is
/// visible when it is wrong: a hash with too few iterations verifies perfectly, and a comparison that
/// short-circuits on the first differing byte returns the right answer. The tests pass. The crack is
/// just cheaper.
///
/// The adapter exists so this type stays behind the Application's port and never reaches a use case.
/// </remarks>
public sealed class PasswordHasherAdapter : IPasswordHasher
{
    private readonly PasswordHasher<User> _hasher = new();

    /// <summary>
    /// The user argument is unused by the v3 hasher — it takes no user-specific input beyond the
    /// password. Passing a throwaway is correct, not a shortcut.
    /// </summary>
    private static readonly User Unused = new()
    {
        Email = string.Empty,
        NormalizedEmail = string.Empty,
        PasswordHash = string.Empty,
    };

    public string Hash(string password) => _hasher.HashPassword(Unused, password);

    public ApplicationResult Verify(string hash, string password)
    {
        var result = _hasher.VerifyHashedPassword(Unused, hash, password);

        return result switch
        {
            MicrosoftResult.Success => ApplicationResult.Success,
            MicrosoftResult.SuccessRehashNeeded => ApplicationResult.SuccessRehashNeeded,
            _ => ApplicationResult.Failed,
        };
    }
}
