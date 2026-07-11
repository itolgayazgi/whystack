namespace WhyStack.Domain.Identity;

/// <summary>
/// The canonical roles. ADR-0005 owns this list; this type reflects the decision, it does not make it.
/// </summary>
/// <remarks>
/// Three are active in the MVP: <see cref="Guest"/>, <see cref="RegisteredUser"/> and
/// <see cref="Administrator"/>. The rest are seeded into the Roles table but grant nothing beyond
/// RegisteredUser until a later sprint binds behaviour to them.
///
/// They are seeded now rather than added later because adding a role to a live system means a
/// migration, a seed and a backfill; adding a row nobody reads costs nothing.
/// </remarks>
public enum RoleName
{
    Guest = 1,
    RegisteredUser = 2,
    PremiumUser = 3,
    Editor = 4,
    Reviewer = 5,
    Translator = 6,
    Administrator = 7,
}

public class Role
{
    public Guid Id { get; init; }

    public required RoleName Name { get; init; }

    /// <summary>Upper-cased, culture-invariant. Lookups compare on this, never on <see cref="Name"/>.</summary>
    public required string NormalizedName { get; init; }

    public string? Description { get; init; }

    /// <summary>A system role is part of the authorization model, not user-managed data. It is never deleted.</summary>
    public bool IsSystemRole { get; init; } = true;

    public DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; set; }

    /// <summary>Roles active in the MVP. The others exist in the table and grant nothing (ADR-0005).</summary>
    public static readonly IReadOnlySet<RoleName> ActiveInMvp = new HashSet<RoleName>
    {
        RoleName.Guest,
        RoleName.RegisteredUser,
        RoleName.Administrator,
    };
}
