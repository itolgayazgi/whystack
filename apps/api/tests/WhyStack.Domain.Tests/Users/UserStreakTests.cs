using WhyStack.Domain.Users;

namespace WhyStack.Domain.Tests.Users;

/// <summary>
/// The streak rule (ADR-0025). Small, and the kind of small that is wrong in production for a year.
/// </summary>
public class UserStreakTests
{
    private static UserStreak On(string date, int current = 1, int longest = 1) => new()
    {
        UserId = Guid.CreateVersion7(),
        CurrentStreak = current,
        LongestStreak = longest,
        LastActiveOn = DateOnly.Parse(date),
    };

    [Fact]
    public void Reading_again_the_same_day_changes_nothing()
    {
        var streak = On("2026-07-16", current: 5, longest: 9);

        streak.Touch(DateOnly.Parse("2026-07-16"));

        // Not 6. A streak counts DAYS, and opening the app twice before lunch is one day.
        Assert.Equal(5, streak.CurrentStreak);
        Assert.Equal(9, streak.LongestStreak);
    }

    [Fact]
    public void Reading_the_next_day_extends_it()
    {
        var streak = On("2026-07-16", current: 5, longest: 9);

        streak.Touch(DateOnly.Parse("2026-07-17"));

        Assert.Equal(6, streak.CurrentStreak);
        Assert.Equal(DateOnly.Parse("2026-07-17"), streak.LastActiveOn);
    }

    [Fact]
    public void Missing_a_day_resets_to_one_not_zero()
    {
        var streak = On("2026-07-16", current: 12, longest: 12);

        streak.Touch(DateOnly.Parse("2026-07-18"));

        // One, because they ARE reading today. Zero would be the product telling somebody who just showed up
        // that they have done nothing.
        Assert.Equal(1, streak.CurrentStreak);
    }

    [Fact]
    public void The_longest_streak_survives_a_reset()
    {
        var streak = On("2026-07-16", current: 12, longest: 12);

        streak.Touch(DateOnly.Parse("2026-07-30"));

        Assert.Equal(1, streak.CurrentStreak);
        Assert.Equal(12, streak.LongestStreak);
    }

    /// <summary>A streak that spans a month boundary is still consecutive days.</summary>
    [Fact]
    public void A_month_boundary_is_not_a_gap()
    {
        var streak = On("2026-07-31", current: 3, longest: 3);

        streak.Touch(DateOnly.Parse("2026-08-01"));

        Assert.Equal(4, streak.CurrentStreak);
    }

    /// <summary>
    /// 29 February is a real day, and a leap year is where date arithmetic goes to die.
    /// </summary>
    [Fact]
    public void A_leap_day_is_not_a_gap()
    {
        var streak = On("2028-02-28", current: 2, longest: 2);

        streak.Touch(DateOnly.Parse("2028-02-29"));
        Assert.Equal(3, streak.CurrentStreak);

        streak.Touch(DateOnly.Parse("2028-03-01"));
        Assert.Equal(4, streak.CurrentStreak);
    }
}
