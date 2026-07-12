namespace WhyStack.Application.Users;

/// <summary>
/// The reading-type scale steps a user may choose (design tokens § "Reading font scale").
/// </summary>
/// <remarks>
/// <b>These numbers are a design token, and they are duplicated here on purpose — under a guard.</b>
///
/// The canonical list lives in <c>packages/theme/src/reading-font-scale.json</c>, because the client
/// needs it too and TypeScript and C# cannot share a module. Reading that file at runtime would put a
/// web API's request validation behind a filesystem read of a frontend package — a deployment
/// dependency nobody would expect and the container would not ship.
///
/// So the constant is compiled, and <c>ReadingFontScaleTokenTests</c> reads the JSON and asserts the
/// two match. Change the token, forget the API, and the build goes red. That test is the only reason
/// this is not the hardcoded design value CLAUDE.md §1.8 forbids.
///
/// <b>Why a set and not a range:</b> type is a scale, not a number. Size, line height and the 4px
/// vertical rhythm are chosen together, so an arbitrary 1.0732× lands the baseline between grid lines
/// on every screen. Four steps are four layouts that can be tested.
/// </remarks>
public static class ReadingFontScale
{
    public const double Default = 1.0;

    public static readonly IReadOnlyList<double> Steps = [0.875, 1.0, 1.25, 1.5];

    /// <summary>
    /// Exact comparison, deliberately — no epsilon, no rounding, no "nearest step".
    ///
    /// The allowed values are exactly representable in binary floating point (0.875 = 2⁻³ + 2⁻² + 2⁻¹
    /// scaled; 1.25 and 1.5 likewise), so a client that sends one of them round-trips through JSON
    /// bit-for-bit. Snapping a near-miss to the closest step would be a silent correction of the
    /// caller's request — the client would believe it set 1.1 and the server would have stored 1.0,
    /// with nothing anywhere saying so.
    /// </summary>
    public static bool IsAllowed(double value) => Steps.Contains(value);

    public static string Describe() => string.Join(", ", Steps);
}
