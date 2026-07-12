using WhyStack.Application.Abstractions;
using WhyStack.Application.Common;

namespace WhyStack.Application.Users.Profile;

/// <summary>
/// `04` — User Profile Foundation. Note what is NOT here: no name, no location, no photo, no birthday.
/// "Avoid unnecessary personal data" is a requirement, and the cheapest way to never leak a field is to
/// never have it.
/// </summary>
public sealed record CurrentUserResult(
    Guid Id,
    string Email,
    string? DisplayName,
    bool IsEmailConfirmed,
    DateTime CreatedAtUtc,
    IReadOnlyCollection<string> Roles);

/// <summary>
/// Who the caller is, according to the DATABASE — not according to their token.
/// </summary>
/// <remarks>
/// The access token already carries the email and the roles, so this endpoint could be answered from
/// the claims without touching the database at all. It deliberately is not.
///
/// A token is a fifteen-minute-old snapshot. Roles baked into it stay effective until it expires; an
/// email confirmed two minutes ago still reads <c>email_confirmed: false</c>. That is a fine trade for
/// AUTHORIZING a request — it is the whole point of a stateless token — but it is the wrong answer for
/// a screen whose entire job is to show the user their own current state. "I just confirmed my email
/// and the app still says I haven't" is a bug report, and it would be a true one.
/// </remarks>
public sealed class GetCurrentUserHandler(IIdentityRepository repository)
{
    public async Task<Result<CurrentUserResult>> HandleAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var user = await repository.FindByIdAsync(userId, cancellationToken);

        // A valid token for a user who no longer exists. Rare, but not impossible: the account was
        // deleted in the last fifteen minutes and the token outlives it. 404 is the truth.
        if (user is null)
        {
            return new Error(ErrorCodes.ResourceNotFound, "This account no longer exists.");
        }

        var roles = await repository.GetRolesAsync(user.Id, cancellationToken);

        return Result<CurrentUserResult>.Success(new CurrentUserResult(
            user.Id,
            user.Email,
            user.DisplayName,
            user.IsEmailConfirmed,
            user.CreatedAtUtc,
            [.. roles.Select(role => role.ToString())]));
    }
}
