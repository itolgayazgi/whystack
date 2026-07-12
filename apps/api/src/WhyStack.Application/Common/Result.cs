namespace WhyStack.Application.Common;

/// <summary>
/// The outcome of a use case. An expected failure — a taken email, a wrong password — is a RESULT,
/// not an exception.
/// </summary>
/// <remarks>
/// Exceptions are for the unexpected. "The user typed the wrong password" is the most expected thing
/// that will ever happen to this system, and throwing for it costs a stack capture on the hot path,
/// hides the failure inside a catch somewhere, and makes the compiler stop reminding you it can fail.
///
/// A Result cannot be ignored: to read <see cref="Value"/> you must first have decided what to do
/// about <see cref="Error"/>.
/// </remarks>
public readonly record struct Result<T>
{
    private readonly T? _value;

    private Result(T value)
    {
        _value = value;
        Error = null;
    }

    private Result(Error error)
    {
        _value = default;
        Error = error;
    }

    public Error? Error { get; }

    public bool IsSuccess => Error is null;

    public T Value =>
        IsSuccess
            ? _value!
            : throw new InvalidOperationException($"Result failed with '{Error!.Code}'; there is no value to read.");

    public static Result<T> Success(T value) => new(value);

    public static Result<T> Failure(Error error) => new(error);

    public static implicit operator Result<T>(Error error) => Failure(error);
}

/// <summary>
/// A failure the API can render. <see cref="Code"/> is the stable, machine-readable identifier `08`
/// requires ("Error codes must be stable. Clients may depend on them.") — it is part of the contract,
/// and renaming one is a breaking change.
/// </summary>
public sealed record Error(string Code, string Message, IReadOnlyDictionary<string, string[]>? FieldErrors = null)
{
    public static Error Validation(IReadOnlyDictionary<string, string[]> fieldErrors) =>
        new(ErrorCodes.ValidationFailed, "One or more validation errors occurred.", fieldErrors);

    public static Error Validation(string field, string message) =>
        Validation(new Dictionary<string, string[]> { [field] = [message] });
}

/// <summary>The approved codes from `08`. Stable by contract.</summary>
public static class ErrorCodes
{
    public const string ValidationFailed = "validation_failed";
    public const string AuthenticationRequired = "authentication_required";
    public const string AccessDenied = "access_denied";
    public const string RateLimitExceeded = "rate_limit_exceeded";

    /// <summary>
    /// Wrong password, unknown account, deleted account — all of them. Deliberately indistinguishable.
    ///
    /// An error that says "no account with that email" is an oracle: it lets anyone test an address
    /// list against the site and learn who has an account here. `04` requires account-enumeration
    /// protection, and this single code is most of it.
    /// </summary>
    public const string InvalidCredentials = "invalid_credentials";

    /// <summary>The account exists but is locked or deactivated. Only ever returned AFTER the password verified.</summary>
    public const string AccountLocked = "account_locked";

    /// <summary>
    /// Unknown, expired, revoked or REPLAYED — all of them, indistinguishably.
    ///
    /// A refresh token is a bearer credential: whoever holds it is the user. Telling the caller which
    /// kind of failure it was tells an attacker how close they are, and telling them "that token was
    /// already used" confirms they are holding a real one.
    /// </summary>
    public const string InvalidRefreshToken = "invalid_refresh_token";

    /// <summary>Expired, spent, or never issued. Indistinguishable, for the same reason as everything else here.</summary>
    public const string InvalidResetToken = "invalid_reset_token";

    public const string InvalidConfirmationToken = "invalid_confirmation_token";

    public const string ResourceNotFound = "resource_not_found";

    /// <summary>
    /// Somebody else changed this row between the caller's read and their write.
    /// </summary>
    /// <remarks>
    /// `07` names preferences as an area where concurrency matters, and the failure it prevents is a
    /// quiet one: a phone and a laptop both open the settings screen, each changes one thing, and the
    /// second save silently reverts the first. Nobody sees an error. The user just finds the setting
    /// they changed this morning has changed back, and has no idea why.
    /// </remarks>
    public const string ConcurrencyConflict = "concurrency_conflict";

    /// <summary>A language we do not have an interface or content for. `08`'s approved code.</summary>
    public const string ContentLanguageNotSupported = "content_language_not_supported";
}
