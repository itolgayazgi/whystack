using WhyStack.Application.Abstractions;
using WhyStack.Application.Common;

namespace WhyStack.Application.Users.Preferences;

public sealed class GetPreferencesHandler(IUserPreferencesRepository repository)
{
    public async Task<Result<UserPreferencesResult>> HandleAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var preferences = await repository.FindByUserIdAsync(userId, cancellationToken);

        if (preferences is null)
        {
            // A 404 for an account that exists means the invariant broke — registration is supposed to
            // create this row in the same transaction as the user (IIdentityRepository.AddPreferences),
            // and the migration backfilled everyone who predates that.
            //
            // It would be easy to "fix" this by creating the row here and returning it. That is exactly
            // what `08` forbids — a GET must not mutate server state — and it would be worse than the
            // 404: it would paper over a broken invariant so thoroughly that nobody would ever learn the
            // registration path had stopped writing preferences (CLAUDE.md §1.6 — never hide a failure).
            return new Error(
                ErrorCodes.ResourceNotFound,
                "No preferences exist for this account.");
        }

        return Result<UserPreferencesResult>.Success(UserPreferencesResult.From(preferences));
    }
}
