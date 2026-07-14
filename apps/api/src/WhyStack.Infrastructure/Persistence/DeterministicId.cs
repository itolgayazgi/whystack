using System.Security.Cryptography;
using System.Text;

namespace WhyStack.Infrastructure.Persistence;

/// <summary>
/// A Guid derived from a string, the same on every machine, forever.
/// </summary>
/// <remarks>
/// <b>Seed data cannot use <c>Guid.NewGuid()</c>.</b> EF Core bakes the value into the migration file, so a
/// new Guid produces a DIFFERENT migration on every developer's machine — and the second one to run against
/// a shared database deletes the first one's rows and inserts its own, taking every foreign key with it.
///
/// The failure is quiet and it is late: the migration applies cleanly, and a topic simply loses its
/// ecosystem.
///
/// So the id is a function of the key. `ecosystem:dotnet` maps to one Guid, on every machine, in every
/// environment, in 2030.
/// </remarks>
public static class DeterministicId
{
    public static Guid For(string key)
    {
        // SHA-256 truncated to 16 bytes. Not a hash for security — a hash for determinism. Collisions are
        // the only risk and 2^-64 across a few dozen seed rows is not one.
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(key));

        return new Guid(hash.AsSpan(0, 16));
    }
}
