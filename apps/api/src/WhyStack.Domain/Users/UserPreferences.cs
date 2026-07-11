namespace WhyStack.Domain.Users;

/// <summary>
/// How the product behaves for one learner (`07` — User Preferences Domain).
/// </summary>
/// <remarks>
/// <see cref="ApplicationLanguageCode"/> and <see cref="ContentLanguageCode"/> are separate columns,
/// and `07` states the rule outright: "Application language and content language must remain separate."
///
/// They are separate because they answer different questions. A Turkish developer may want the
/// interface in Turkish and still read a topic in English — because the Turkish translation does not
/// exist yet, or because they prefer the original. One column would force the product to lie about one
/// of the two, and the UI already refuses to hide that (the language fallback notice in the client).
/// </remarks>
public class UserPreferences
{
    public Guid Id { get; init; }

    public required Guid UserId { get; init; }

    /// <summary>Interface language. On first launch: Turkish device → "tr", everything else → "en" (`04`).</summary>
    public required string ApplicationLanguageCode { get; set; }

    /// <summary>Preferred language of the educational content. Independent of the interface.</summary>
    public required string ContentLanguageCode { get; set; }

    public ThemeMode ThemeMode { get; set; } = ThemeMode.System;

    /// <summary>Multiplier on the reading type. Accessibility, not decoration.</summary>
    public double ReadingFontScale { get; set; } = 1.0;

    /// <summary>
    /// The user's own override. It does NOT replace the operating system's reduced-motion setting —
    /// the client honours that regardless. This lets someone ask for stillness on a device that does
    /// not offer it, and never lets an app override someone who already asked.
    /// </summary>
    public bool ReducedMotionEnabled { get; set; }

    public SkillLevel? PreferredSkillLevel { get; set; }

    public DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; set; }

    /// <summary>
    /// `07` names preferences as an area where concurrency matters — a phone and a laptop both syncing
    /// settings will otherwise silently overwrite each other, and the loser never knows.
    /// </summary>
    public byte[]? RowVersion { get; set; }
}

public enum ThemeMode
{
    /// <summary>Follow the device. The default, and what most people want without being asked.</summary>
    System = 1,
    Light = 2,
    Dark = 3,
}

public enum SkillLevel
{
    Junior = 1,
    MidLevel = 2,
    Senior = 3,
    Expert = 4,
}
