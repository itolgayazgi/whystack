using WhyStack.Application.Common;

namespace WhyStack.Application.Progress;

/// <summary>
/// Record where a reader is (ADR-0025), and touch their streak while they are here.
/// </summary>
/// <remarks>
/// <b>The server does not trust the number.</b> The client says "I am at block 7 of async/.NET"; the client is
/// a program anybody can replace with curl, and a progress bar past 100% is the least of what a hand-written
/// request would do. The position is clamped to the topic's real block count, and an unknown topic or
/// ecosystem is refused rather than stored.
/// </remarks>
public sealed class RecordProgressHandler(IProgressRepository repository, TimeProvider clock)
{
    public async Task<Result<RecordOutcome>> HandleAsync(
        Guid userId,
        string slug,
        string? ecosystemKey,
        int lastBlockOrder,
        bool? completed,
        CancellationToken cancellationToken)
    {
        if (lastBlockOrder < 0)
        {
            return Error.Validation("lastBlockOrder", "A position cannot be negative.");
        }

        // The reader's day, from the server's clock. A client-supplied date would let anybody keep a streak
        // alive by lying about what day it is — which is the whole mechanic, for free.
        var today = DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime);

        var outcome = await repository.RecordAsync(
            userId, slug, ecosystemKey, lastBlockOrder, completed, today, cancellationToken);

        if (outcome is null)
        {
            return new Error(ErrorCodes.ResourceNotFound, "No such topic, or no such ecosystem on it.");
        }

        return Result<RecordOutcome>.Success(outcome);
    }
}

/// <summary>The home screen's state in one call.</summary>
public sealed class GetHomeHandler(IProgressRepository repository)
{
    public Task<HomeSnapshot> HandleAsync(
        Guid userId,
        string? ecosystemKey,
        string language,
        CancellationToken cancellationToken) =>
        repository.HomeAsync(userId, ecosystemKey, language, cancellationToken);
}
