namespace WhyStack.Domain.Identity;

/// <summary>Maps a user to a role. `07`: a user may have several.</summary>
public class UserRole
{
    public required Guid UserId { get; init; }
    public required Guid RoleId { get; init; }

    public DateTime AssignedAtUtc { get; init; }

    /// <summary>
    /// Null when the system assigned it — the RegisteredUser role granted at registration has no human
    /// behind it. `07` requires role changes to be audited, and "who did this" is the first question.
    /// </summary>
    public Guid? AssignedByUserId { get; init; }

    public User? User { get; init; }
    public Role? Role { get; init; }
}
