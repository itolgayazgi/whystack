using Microsoft.EntityFrameworkCore;
using WhyStack.Application.Progress;
using WhyStack.Domain.Content;
using WhyStack.Domain.Users;

namespace WhyStack.Infrastructure.Persistence;

public sealed class ProgressRepository(WhyStackDbContext context, TimeProvider clock) : IProgressRepository
{
    /// <summary>Published only. Progress against a draft would be progress against something nobody can read.</summary>
    private IQueryable<Topic> Readable() =>
        context.Topics.Where(topic =>
            topic.Versions
                .OrderByDescending(version => version.VersionNumber)
                .Select(version => version.Status)
                .First() == ContentStatus.Published);

    public async Task<RecordOutcome?> RecordAsync(
        Guid userId,
        string slug,
        string? ecosystemKey,
        int lastBlockOrder,
        bool? completed,
        DateOnly today,
        CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow().UtcDateTime;

        var topic = await Readable()
            .Where(candidate => candidate.Slug == slug)
            .Select(candidate => new
            {
                candidate.Id,
                candidate.DefaultLevel,

                // The topic's real block count in the canonical language, so the position can be clamped to
                // something that exists. Shared blocks plus this ecosystem's — the same merge the reader sees
                // (ADR-0024), because a position past the end of THEIR flow is still past the end.
                Blocks = candidate.Versions
                    .OrderByDescending(version => version.VersionNumber)
                    .Select(version => version.Blocks
                        .Count(block =>
                            block.LanguageCode == version.CanonicalLanguageCode
                            && (block.EcosystemKey == null || block.EcosystemKey == ecosystemKey)))
                    .First(),
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (topic is null) return null;

        // A named ecosystem must exist. Storing progress against "dotnett" would be a row that never matches
        // anything again — invisible, and permanent.
        if (ecosystemKey is not null
            && !await context.Ecosystems.AnyAsync(eco => eco.Key == ecosystemKey, cancellationToken))
        {
            return null;
        }

        // CLAMPED, not rejected. A client that is one ahead of a freshly edited topic is a client bug, not an
        // attack, and refusing would lose a reader's place over an off-by-one.
        var position = Math.Clamp(lastBlockOrder, 0, Math.Max(topic.Blocks, 0));

        var progress = await context.UserTopicProgress.SingleOrDefaultAsync(
            candidate => candidate.UserId == userId
                && candidate.TopicId == topic.Id
                && candidate.EcosystemKey == ecosystemKey,
            cancellationToken);

        if (progress is null)
        {
            progress = new UserTopicProgress
            {
                Id = Guid.CreateVersion7(),
                UserId = userId,
                TopicId = topic.Id,
                EcosystemKey = ecosystemKey,
                LastBlockOrder = position,
                StartedAtUtc = now,
                UpdatedAtUtc = now,
            };

            context.UserTopicProgress.Add(progress);
        }
        else
        {
            // The FURTHEST point, not the latest. Scrolling back up to re-read the hook must not move
            // "kaldığın yer" backwards — a reader who checks something and returns has not lost their place.
            progress.LastBlockOrder = Math.Max(progress.LastBlockOrder, position);
            progress.UpdatedAtUtc = now;
        }

        if (completed == true && !progress.IsCompleted)
        {
            progress.IsCompleted = true;
            progress.CompletedAtUtc = now;
        }
        else if (completed == false)
        {
            // Un-completing is allowed: "Needs Review" is a thing a reader decides, and the transcript of
            // when they first finished is not worth trapping them in a claim.
            progress.IsCompleted = false;
            progress.CompletedAtUtc = null;
        }

        await TouchStreakAsync(userId, today, cancellationToken);
        await EnterLevelAsync(userId, topic.DefaultLevel, now, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        return new RecordOutcome(progress.LastBlockOrder, progress.IsCompleted, topic.Blocks);
    }

    /// <summary>
    /// Stamps the moment a reader first arrives at a basamak. Once — never moved.
    /// </summary>
    /// <remarks>
    /// This is what stops a reader's percentage falling because we published something. The denominator for
    /// a level is the stops that existed when they GOT there; anything published later is "1 yeni", a
    /// reward rather than a debt.
    ///
    /// Stamped on first CONTACT with a stop at that level, not on completion. Somebody who has opened one
    /// Mid topic is at Mid — waiting for them to finish one would leave the denominator floating for as long
    /// as they are still reading, which is the whole time it matters.
    /// </remarks>
    private async Task EnterLevelAsync(
        Guid userId,
        SkillLevel level,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var already = await context.UserLevelBaselines
            .AnyAsync(baseline => baseline.UserId == userId && baseline.Level == level, cancellationToken);

        if (already) return;

        context.UserLevelBaselines.Add(new UserLevelBaseline
        {
            UserId = userId,
            Level = level,
            EnteredAtUtc = now,
        });
    }

    private async Task TouchStreakAsync(Guid userId, DateOnly today, CancellationToken cancellationToken)
    {
        var streak = await context.UserStreaks
            .SingleOrDefaultAsync(candidate => candidate.UserId == userId, cancellationToken);

        if (streak is null)
        {
            context.UserStreaks.Add(new UserStreak
            {
                UserId = userId,
                CurrentStreak = 1,
                LongestStreak = 1,
                LastActiveOn = today,
            });

            return;
        }

        // The rule lives on the entity. This method's only job is to find the row and save it.
        streak.Touch(today);
    }

    public async Task<HomeSnapshot> HomeAsync(
        Guid userId,
        string? ecosystemKey,
        string language,
        CancellationToken cancellationToken)
    {
        var streak = await context.UserStreaks
            .AsNoTracking()
            .Where(candidate => candidate.UserId == userId)
            .Select(candidate => new StreakView(candidate.CurrentStreak, candidate.LongestStreak))
            .SingleOrDefaultAsync(cancellationToken);

        // "Kaldığın yer" — the most recently touched station that is NOT finished. Offering to continue
        // something they completed would be the home screen sending them backwards.
        var resume = await context.UserTopicProgress
            .AsNoTracking()
            .Where(progress => progress.UserId == userId && !progress.IsCompleted)
            .OrderByDescending(progress => progress.UpdatedAtUtc)
            .Select(progress => new
            {
                progress.Topic!.Slug,
                progress.EcosystemKey,
                progress.LastBlockOrder,
                Version = progress.Topic.Versions
                    .OrderByDescending(version => version.VersionNumber)
                    .Select(version => new
                    {
                        version.EstimatedReadingMinutes,
                        version.CanonicalLanguageCode,
                        Title = version.Translations
                            .Where(translation => translation.LanguageCode == language)
                            .Select(translation => translation.Title)
                            .FirstOrDefault(),
                        Blocks = version.Blocks.Count(block =>
                            block.LanguageCode == version.CanonicalLanguageCode
                            && (block.EcosystemKey == null || block.EcosystemKey == progress.EcosystemKey)),
                    })
                    .First(),
                DefaultTitle = progress.Topic.DefaultTitle,
            })
            .FirstOrDefaultAsync(cancellationToken);

        // The basamak chart, measured against the corpus AS IT WAS WHEN THE READER ARRIVED.
        //
        // Not as it stands now. Publishing one Junior topic would otherwise push every Junior reader further
        // from the top than they were yesterday — the platform's own productivity, charged to the people who
        // read the most. A stop published after they got here is "1 yeni", never a debt.
        var baselines = await context.UserLevelBaselines
            .AsNoTracking()
            .Where(baseline => baseline.UserId == userId)
            .ToDictionaryAsync(baseline => baseline.Level, baseline => baseline.EnteredAtUtc, cancellationToken);

        var counts = await Readable()
            .Select(topic => new
            {
                topic.DefaultLevel,

                // The version that is live. Its PublishedAtUtc is what "already existed" is measured on.
                PublishedAtUtc = topic.Versions
                    .OrderByDescending(version => version.VersionNumber)
                    .Select(version => version.PublishedAtUtc)
                    .First(),
            })
            .ToListAsync(cancellationToken);

        var totals = counts
            .GroupBy(entry => entry.DefaultLevel)
            .Select(group =>
            {
                // No baseline means the reader has never touched this level. There is no history to protect,
                // so the honest denominator is everything that is published — that is what they would find
                // if they went there now.
                var entered = baselines.TryGetValue(group.Key, out var at) ? at : (DateTime?)null;

                var inBaseline = entered is null
                    ? group.Count()
                    : group.Count(entry => entry.PublishedAtUtc is not null && entry.PublishedAtUtc <= entered);

                return new
                {
                    Level = group.Key,
                    Total = inBaseline,
                    Fresh = group.Count() - inBaseline,
                };
            })
            .ToList();

        var done = await context.UserTopicProgress
            .AsNoTracking()
            .Where(progress => progress.UserId == userId && progress.IsCompleted)
            .GroupBy(progress => progress.Topic!.DefaultLevel)
            .Select(group => new { Level = group.Key, Completed = group.Count() })
            .ToListAsync(cancellationToken);

        var levels = totals
            .OrderBy(entry => entry.Level)
            .Select(entry => new LevelProgressView(
                entry.Level.ToString(),

                // Clamped to the baseline. A reader who finished ten stops and then read an eleventh that we
                // published afterwards would otherwise read "11/10" — the guard against their own reward.
                Math.Min(done.FirstOrDefault(finished => finished.Level == entry.Level)?.Completed ?? 0, entry.Total),
                entry.Total,
                entry.Fresh))
            .ToList();

        // What to read next: published topics this reader has not finished. Not a recommendation engine —
        // the roadmap is its own thing, and until it exists this is honest rather than clever.
        var started = await context.UserTopicProgress
            .AsNoTracking()
            .Where(progress => progress.UserId == userId && progress.IsCompleted)
            .Select(progress => progress.TopicId)
            .ToListAsync(cancellationToken);

        var next = await Readable()
            .Where(topic => !started.Contains(topic.Id))
            .OrderBy(topic => topic.DefaultLevel)
            .ThenBy(topic => topic.DefaultTitle)
            .Take(5)
            .Select(topic => new
            {
                topic.Slug,
                topic.DefaultTitle,
                topic.DefaultLevel,
                LineName = topic.Line!.Name,
                Version = topic.Versions
                    .OrderByDescending(version => version.VersionNumber)
                    .Select(version => new
                    {
                        version.EstimatedReadingMinutes,
                        Title = version.Translations
                            .Where(translation => translation.LanguageCode == language)
                            .Select(translation => translation.Title)
                            .FirstOrDefault(),
                    })
                    .First(),
            })
            .ToListAsync(cancellationToken);

        return new HomeSnapshot(
            streak ?? new StreakView(0, 0),
            resume is null
                ? null
                : new ContinueView(
                    resume.Slug,
                    resume.Version.Title ?? resume.DefaultTitle,
                    resume.EcosystemKey,
                    resume.LastBlockOrder,
                    resume.Version.Blocks,
                    resume.Version.EstimatedReadingMinutes),
            levels,
            [
                .. next.Select(topic => new NextTopicView(
                    topic.Slug,
                    topic.Version.Title ?? topic.DefaultTitle,
                    topic.DefaultLevel.ToString(),
                    topic.LineName,
                    topic.Version.EstimatedReadingMinutes)),
            ]);
    }
}
