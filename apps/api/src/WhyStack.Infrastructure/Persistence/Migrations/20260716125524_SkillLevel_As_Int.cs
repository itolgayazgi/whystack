using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WhyStack.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// SkillLevel becomes a spaced integer (ADR-0026).
    /// </summary>
    /// <remarks>
    /// HAND-WRITTEN, because what EF scaffolded could not run.
    ///
    /// It emitted a bare <c>AlterColumn&lt;int&gt;</c> on a column holding 'Junior' — SQL Server cannot
    /// convert that to an int and would have failed the deployment on the first row. It also left the two
    /// indexes on DefaultLevel in place, and SQL Server refuses to alter an indexed column at all. So the
    /// order here is the only one that works: translate the text to its number, drop the indexes, change the
    /// type, put the indexes back.
    ///
    /// The RAISERROR is the point of the whole file. A backfill written as a plain CASE with no ELSE turns
    /// an unrecognised value into NULL, and NULL into 0 the moment the column is made NOT NULL — which is
    /// exactly how Archetype ended up holding 0, a value its enum does not define, without anybody noticing.
    /// A migration that cannot explain a row must stop, not guess.
    /// </remarks>
    public partial class SkillLevel_As_Int : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Refuse before touching anything. Inside the transaction the migration already runs in, so a
            // corpus with one unreadable row leaves the database exactly as it was found.
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM [Topics] WHERE [DefaultLevel] NOT IN ('Junior','MidLevel','Senior','Expert'))
                    THROW 50000, 'Topics.DefaultLevel holds a value this migration does not recognise. Nothing was changed.', 1;

                IF EXISTS (SELECT 1 FROM [UserPreferences]
                           WHERE [PreferredSkillLevel] IS NOT NULL
                             AND [PreferredSkillLevel] NOT IN ('Junior','MidLevel','Senior','Expert'))
                    THROW 50000, 'UserPreferences.PreferredSkillLevel holds a value this migration does not recognise. Nothing was changed.', 1;
                """);

            // Text to the TEXT of the number, so the ALTER below is a conversion SQL Server can do.
            migrationBuilder.Sql("""
                UPDATE [Topics] SET [DefaultLevel] = CASE [DefaultLevel]
                    WHEN 'Junior' THEN '10' WHEN 'MidLevel' THEN '20'
                    WHEN 'Senior' THEN '30' WHEN 'Expert' THEN '40' END;

                UPDATE [UserPreferences] SET [PreferredSkillLevel] = CASE [PreferredSkillLevel]
                    WHEN 'Junior' THEN '10' WHEN 'MidLevel' THEN '20'
                    WHEN 'Senior' THEN '30' WHEN 'Expert' THEN '40' END
                WHERE [PreferredSkillLevel] IS NOT NULL;
                """);

            migrationBuilder.DropIndex(name: "IX_Topics_DomainId_DefaultLevel", table: "Topics");
            migrationBuilder.DropIndex(name: "IX_Topics_SubAreaId_DefaultLevel", table: "Topics");

            migrationBuilder.AlterColumn<int>(
                name: "DefaultLevel",
                table: "Topics",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(16)",
                oldMaxLength: 16);

            migrationBuilder.AlterColumn<int>(
                name: "PreferredSkillLevel",
                table: "UserPreferences",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(16)",
                oldMaxLength: 16,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Topics_DomainId_DefaultLevel",
                table: "Topics",
                columns: ["DomainId", "DefaultLevel"]);

            migrationBuilder.CreateIndex(
                name: "IX_Topics_SubAreaId_DefaultLevel",
                table: "Topics",
                columns: ["SubAreaId", "DefaultLevel"],
                filter: "[SubAreaId] IS NOT NULL");

            // Bought back on purpose. An int column takes any int; string storage made a meaningless value
            // impossible for free, and this is what replaces that guarantee (ADR-0026).
            migrationBuilder.AddCheckConstraint(
                name: "CK_Topics_DefaultLevel",
                table: "Topics",
                sql: "[DefaultLevel] IN (10, 20, 30, 40)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_UserPreferences_PreferredSkillLevel",
                table: "UserPreferences",
                sql: "[PreferredSkillLevel] IS NULL OR [PreferredSkillLevel] IN (10, 20, 30, 40)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(name: "CK_Topics_DefaultLevel", table: "Topics");
            migrationBuilder.DropCheckConstraint(name: "CK_UserPreferences_PreferredSkillLevel", table: "UserPreferences");

            migrationBuilder.DropIndex(name: "IX_Topics_DomainId_DefaultLevel", table: "Topics");
            migrationBuilder.DropIndex(name: "IX_Topics_SubAreaId_DefaultLevel", table: "Topics");

            migrationBuilder.AlterColumn<string>(
                name: "DefaultLevel",
                table: "Topics",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "PreferredSkillLevel",
                table: "UserPreferences",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            // Back to the names. Down is not a formality — it is what someone runs at 3am when Up went wrong,
            // and one that leaves '10' in a column the code reads as 'Junior' is worse than no Down at all.
            migrationBuilder.Sql("""
                UPDATE [Topics] SET [DefaultLevel] = CASE [DefaultLevel]
                    WHEN '10' THEN 'Junior' WHEN '20' THEN 'MidLevel'
                    WHEN '30' THEN 'Senior' WHEN '40' THEN 'Expert' END;

                UPDATE [UserPreferences] SET [PreferredSkillLevel] = CASE [PreferredSkillLevel]
                    WHEN '10' THEN 'Junior' WHEN '20' THEN 'MidLevel'
                    WHEN '30' THEN 'Senior' WHEN '40' THEN 'Expert' END
                WHERE [PreferredSkillLevel] IS NOT NULL;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Topics_DomainId_DefaultLevel",
                table: "Topics",
                columns: ["DomainId", "DefaultLevel"]);

            migrationBuilder.CreateIndex(
                name: "IX_Topics_SubAreaId_DefaultLevel",
                table: "Topics",
                columns: ["SubAreaId", "DefaultLevel"],
                filter: "[SubAreaId] IS NOT NULL");
        }
    }
}
