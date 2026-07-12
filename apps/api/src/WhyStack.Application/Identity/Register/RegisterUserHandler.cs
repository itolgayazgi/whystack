using WhyStack.Application.Abstractions;
using WhyStack.Application.Common;
using WhyStack.Application.Identity.Tokens;
using WhyStack.Domain.Identity;

namespace WhyStack.Application.Identity.Register;

/// <summary>
/// Email and Password are nullable, and that is not sloppiness — it is the truth. JSON can deliver
/// <c>null</c> for any field, so a non-nullable string here would be a lie the compiler believes and
/// the runtime disproves with a NullReferenceException on the first malformed request.
/// </summary>
public sealed record RegisterUserCommand(
    string? Email,
    string? Password,
    string? DisplayName,
    string? IpAddressHash,
    string? UserAgentHash);

/// <summary>
/// Deliberately says nothing about whether the address was already taken.
/// </summary>
public sealed record RegisterUserResult(string Message);

/// <summary>
/// Registration, written so that it cannot be used to find out who has an account here.
/// </summary>
/// <remarks>
/// <b>The response is identical whether the email was free or taken.</b> `04` requires account
/// enumeration protection, and registration is the easiest place to lose it: returning
/// <c>409 email already registered</c> hands anyone a free oracle — feed it a list of addresses, and
/// it tells you which of them belong to people who use this site. For a site about what someone is
/// learning, that is a meaningful thing to leak.
///
/// The honest alternative to telling the ATTACKER is telling the OWNER: when the address is already
/// registered, we send mail to it saying "someone tried to register with your address; you already
/// have an account". The person who owns the inbox learns everything. The person who does not learns
/// nothing. That is the whole trick, and it is why this cannot be built without a mail port.
///
/// The cost is real and worth stating: a user who genuinely forgot they had an account sees "check
/// your email" instead of "you already have an account". They then get an email that says exactly
/// that. It is one extra step for the honest case, in exchange for closing the oracle for everyone.
/// </remarks>
public sealed class RegisterUserHandler(
    IIdentityRepository repository,
    IPasswordHasher passwordHasher,
    SingleUseTokenService tokens,
    IEmailSender emailSender,
    IAppLinks links,
    IClock clock)
{
    private const string SameAnswerEitherWay =
        "If that address can be registered, we have sent it a confirmation email.";

    public async Task<Result<RegisterUserResult>> HandleAsync(
        RegisterUserCommand command,
        CancellationToken cancellationToken)
    {
        var fieldErrors = new Dictionary<string, string[]>();

        if (!EmailAddress.LooksValid(command.Email))
        {
            fieldErrors["email"] = ["A valid email address is required."];
        }

        var passwordFailures = PasswordPolicy.Validate(command.Password, command.Email ?? string.Empty);
        if (passwordFailures.Count > 0)
        {
            fieldErrors["password"] = [.. passwordFailures];
        }

        if (command.DisplayName is { Length: > 64 })
        {
            fieldErrors["displayName"] = ["Display name must be at most 64 characters."];
        }

        // Validation errors ARE told to the caller. They are about the request, not about who exists:
        // "that is not an email address" leaks nothing, and withholding it would only punish honest users.
        if (fieldErrors.Count > 0)
        {
            return Error.Validation(fieldErrors);
        }

        // Past validation, both are known non-null — LooksValid and the password policy both reject
        // null. The locals say so to the compiler once, instead of every line below asserting it.
        var email = command.Email!;
        var password = command.Password!;

        var normalizedEmail = EmailAddress.Normalize(email);
        var now = clock.UtcNow;

        if (await repository.EmailExistsAsync(normalizedEmail, cancellationToken))
        {
            // Not an error. The caller gets the same answer as a successful registration, and the
            // person who actually owns the inbox gets the truth.
            await emailSender.SendAsync(
                new EmailMessage(
                    To: EmailAddress.Format(email),
                    Subject: "Someone tried to register with your email",
                    Body: """
                        Someone tried to create a WhyStack account with this address.

                        You already have an account here.

                        If this was you: sign in instead, or reset your password if you have forgotten it.

                        If it was not you: no action is needed. Nothing has changed, and no new account
                        was created.
                        """),
                cancellationToken);

            return Result<RegisterUserResult>.Success(new RegisterUserResult(SameAnswerEitherWay));
        }

        var user = new User
        {
            // Version 7: time-ordered. A random GUID as a clustered key fragments the index on every
            // insert, because each new row lands in the middle of the table rather than at the end.
            // On a table that only ever grows, that is a slow, permanent tax nobody attributes to the id.
            Id = Guid.CreateVersion7(),
            Email = EmailAddress.Format(email),
            NormalizedEmail = normalizedEmail,
            PasswordHash = passwordHasher.Hash(password),
            DisplayName = string.IsNullOrWhiteSpace(command.DisplayName) ? null : command.DisplayName.Trim(),
            IsEmailConfirmed = false,
            IsActive = true,
            CreatedAtUtc = now,
        };

        repository.AddUser(user);

        repository.AddUserRole(new UserRole
        {
            UserId = user.Id,
            RoleId = await repository.GetRoleIdAsync(RoleName.RegisteredUser, cancellationToken),
            AssignedAtUtc = now,
            AssignedByUserId = null, // the system did this; no human is behind it
        });

        repository.AddLoginEvent(new UserLoginEvent
        {
            Id = Guid.CreateVersion7(),
            UserId = user.Id,
            Email = user.Email,
            EventType = LoginEventType.RegistrationSucceeded,
            IsSuccessful = true,
            IpAddressHash = command.IpAddressHash,
            UserAgentHash = command.UserAgentHash,
            CreatedAtUtc = now,
        });

        var confirmationToken = await tokens.IssueEmailConfirmationAsync(user, cancellationToken);

        // One SaveChanges, one transaction. If the role assignment fails, the user is not created
        // either — a user with no role is an account that exists and can do nothing, and nobody would
        // ever find out why.
        //
        // The unique index can still reject this: EmailExistsAsync above and this insert are not
        // atomic, and two simultaneous registrations for the same address both pass the check. The
        // database is what settles it, and the endpoint maps that collision back onto the same
        // indistinguishable answer — so even the race does not leak.
        await repository.SaveChangesAsync(cancellationToken);

        // Sent AFTER the save, not before. An email promising a link that does not exist because the
        // transaction rolled back is worse than a slow email — the user clicks, it fails, and they have
        // no idea whether they have an account at all.
        await emailSender.SendAsync(
            new EmailMessage(
                To: user.Email,
                Subject: "Confirm your WhyStack account",
                Body: $"""
                    Welcome to WhyStack. Confirm this address to finish setting up your account.

                    {links.ConfirmEmail(confirmationToken)}

                    The link works once, and expires in {SingleUseTokenService.EmailConfirmationLifetime.TotalHours:0} hours.

                    You can sign in before confirming — confirmation just unlocks everything.
                    """),
            cancellationToken);

        return Result<RegisterUserResult>.Success(new RegisterUserResult(SameAnswerEitherWay));
    }
}
