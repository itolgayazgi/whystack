namespace WhyStack.Domain.Identity;

/// <summary>
/// A user account. Columns follow <c>07-database-design.md</c> (Identity Domain).
/// </summary>
/// <remarks>
/// The Domain layer depends on nothing — no EF Core, no ASP.NET, no attributes. That is not purity for
/// its own sake: it is what lets the rules below be tested without a database, and what stops a
/// persistence concern from quietly becoming a business rule.
///
/// Deliberately minimal. `07`: "User records should avoid unnecessary personal data", and
/// "Display name should not be required for basic learning." There is no name, no phone, no birthday,
/// no avatar. Data you never collect cannot leak.
/// </remarks>
public class User
{
    public Guid Id { get; init; }

    /// <summary>As the user typed it. Shown back to them; never used for lookup.</summary>
    public required string Email { get; set; }

    /// <summary>
    /// Upper-cased, culture-invariant. Every lookup and the uniqueness index use THIS.
    ///
    /// Two reasons. Email local-parts are case-sensitive in the RFC and case-insensitive in every mail
    /// server anyone actually uses, so <c>Ada@x.com</c> and <c>ada@x.com</c> must not be two accounts.
    /// And <c>ToUpperInvariant</c>, not <c>ToUpper</c>: in a Turkish locale, <c>i</c> upper-cases to
    /// <c>İ</c>, and "the Turkish I problem" would let the same address register twice on a machine
    /// with a Turkish culture and not on one without.
    /// </summary>
    public required string NormalizedEmail { get; set; }

    /// <summary>
    /// `07`'s column list calls this <c>EmailConfirmed</c>, but the same document's boolean naming rule
    /// requires an <c>Is</c> prefix and names bare participles as bad. The rule wins over the example.
    /// </summary>
    public bool IsEmailConfirmed { get; set; }

    /// <summary>Opaque. Produced by IPasswordHasher; never compared with <c>==</c>, never logged.</summary>
    public required string PasswordHash { get; set; }

    public string? DisplayName { get; set; }

    /// <summary>False disables sign-in without deleting anything. Set by an administrator.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Set by the system after repeated failed sign-ins — not by an administrator, and not permanently.
    /// <see cref="LockedUntilUtc"/> is when it lifts.
    /// </summary>
    public bool IsLocked { get; set; }

    /// <summary>
    /// Not in `07`'s column list, and required by it anyway: `04` mandates lockout, and a lock with no
    /// expiry is a denial-of-service anyone can trigger against any account by guessing badly at it.
    /// </summary>
    public DateTime? LockedUntilUtc { get; set; }

    public int FailedLoginAttempts { get; set; }

    public DateTime? LastLoginAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; set; }

    /// <summary>
    /// Soft delete. Null means alive. There is no separate <c>IsDeleted</c> flag on purpose: two columns
    /// meaning one thing is two columns that can disagree, and they eventually do.
    ///
    /// A privacy erasure request is NOT this (`07`): it anonymises or hard-deletes, and overrides soft
    /// delete. That is a later sprint, and it is not this field.
    /// </summary>
    public DateTime? DeletedAtUtc { get; set; }

    /// <summary>Optimistic concurrency. Two admins editing the same user must not silently overwrite each other.</summary>
    public byte[]? RowVersion { get; set; }

    public ICollection<UserRole> UserRoles { get; init; } = [];

    public bool IsDeleted => DeletedAtUtc is not null;

    /// <summary>Can this account sign in right now? Every reason to say no, in one place.</summary>
    public bool CanSignIn(DateTime nowUtc) =>
        IsActive && !IsDeleted && !IsLockedAt(nowUtc);

    public bool IsLockedAt(DateTime nowUtc) =>
        IsLocked && (LockedUntilUtc is null || LockedUntilUtc > nowUtc);
}
