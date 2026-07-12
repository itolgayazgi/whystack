using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WhyStack.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Gives every existing account the preferences row it should always have had.
    /// </summary>
    /// <remarks>
    /// <para>A DATA migration, not a schema one — the table has existed since Schema_Identity, but
    /// nothing ever wrote to it. Registration now creates the row in the same transaction as the user
    /// (RegisterUserHandler), so every account created from here on has one. This is for the ones
    /// created before that.</para>
    ///
    /// <para><b>Why it cannot be fixed lazily instead.</b> The obvious alternative — have
    /// <c>GET /users/me/preferences</c> create the row when it finds none — is forbidden by `08`: a GET
    /// must not mutate server state. And it would be the worse engineering answer anyway, because it
    /// would hide a broken invariant so completely that nobody would ever discover the registration path
    /// had stopped writing preferences.</para>
    ///
    /// <para><b>The defaults match registration's,</b> English included — these accounts predate any
    /// device locale being collected, so there is nothing better to infer from. Guessing the language
    /// from an old login's IP address would be unreliable and creepy in equal measure. The user changes
    /// it on the preferences screen; until they do, the stored value is at least honest.</para>
    /// </remarks>
    public partial class Data_BackfillUserPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // WHERE NOT EXISTS, so running it twice is harmless. A data migration that cannot be re-run
            // safely is one that has to be right the first time against a database you cannot see.
            //
            // CreatedAtUtc is copied from the USER, not set to now. The invariant being repaired is
            // "preferences exist from the moment the account does", and dating them today would record
            // the repair rather than the fact.
            //
            // ThemeMode is written as the STRING 'System', because that is how the column stores it
            // (HasConversion<string>). CLAUDE.md §4 forbids numeric enum values, and that applies to the
            // column just as much as to the wire.
            //
            // RowVersion is absent on purpose: SQL Server generates it, and naming it here is an error.
            //
            // NEWID() rather than a time-ordered GUID: T-SQL has no v7, and NEWSEQUENTIALID() is legal
            // only as a column default. This touches the accounts that already exist — a bounded,
            // one-time set — so the index fragmentation a random key causes is paid once and never
            // again. Every row inserted after this one comes from Guid.CreateVersion7().
            migrationBuilder.Sql(
                """
                INSERT INTO [UserPreferences]
                    ([Id], [UserId], [ApplicationLanguageCode], [ContentLanguageCode],
                     [ThemeMode], [ReadingFontScale], [ReducedMotionEnabled],
                     [PreferredSkillLevel], [CreatedAtUtc], [UpdatedAtUtc])
                SELECT
                    NEWID(), [u].[Id], 'en', 'en',
                    'System', 1.0, 0,
                    NULL, [u].[CreatedAtUtc], NULL
                FROM [Users] AS [u]
                WHERE NOT EXISTS (
                    SELECT 1 FROM [UserPreferences] AS [p] WHERE [p].[UserId] = [u].[Id]);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Deliberately empty — the careful choice, not the lazy one.
            //
            // A symmetrical Down would delete from UserPreferences. But nothing distinguishes a row this
            // migration created from one a user has since edited, so reverting would silently destroy
            // real settings belonging to real people — in order to "undo" a change whose only effect was
            // to give them a row they were always supposed to have.
            //
            // Rolling this back leaves the rows in place. Leaving data alone is the correct behaviour
            // for a repair, and a Down that deletes user data is a destructive migration — a human
            // decision, not one this file gets to make (CLAUDE.md §6).
        }
    }
}
