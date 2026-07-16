using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WhyStack.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Archetype becomes a STRING, and the invalid values it already held are repaired.
    /// </summary>
    /// <remarks>
    /// It shipped as an int because the configuration forgot the conversion every other enum here has. That is
    /// not a style point: an int column silently changes meaning the day somebody reorders the enum, in data
    /// nobody re-reads. Worse, the column defaulted to 0 — which <c>Archetype</c> does not define — so the API
    /// was serving <c>"archetype": 0</c> as a topic's type.
    ///
    /// The ALTER turns those into the string "0", which is still not a type. The UPDATE is what actually fixes
    /// the data: every existing topic becomes a Concept until an editor says otherwise.
    /// </remarks>
    public partial class Archetype_As_String : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Archetype",
                table: "Topics",
                type: "nvarchar(24)",
                maxLength: 24,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            // Anything that is not a defined Archetype — "0" from the int default, or a value from a future
            // rollback — becomes Concept. Named explicitly rather than "WHERE Archetype = '0'", so a column
            // holding any other junk is repaired too.
            migrationBuilder.Sql(
                """
                UPDATE Topics
                   SET Archetype = N'Concept'
                 WHERE Archetype NOT IN (
                       N'Concept', N'Mechanism', N'Comparison', N'Incident', N'Pattern', N'Workshop');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // The names cannot be cast back to int, so they are mapped to their enum values first — otherwise
            // the ALTER below fails on the first row that says "Concept".
            migrationBuilder.Sql(
                """
                UPDATE Topics SET Archetype =
                    CASE Archetype
                        WHEN N'Concept'    THEN N'1'
                        WHEN N'Mechanism'  THEN N'2'
                        WHEN N'Comparison' THEN N'3'
                        WHEN N'Incident'   THEN N'4'
                        WHEN N'Pattern'    THEN N'5'
                        WHEN N'Workshop'   THEN N'6'
                        ELSE N'1'
                    END;
                """);

            migrationBuilder.AlterColumn<int>(
                name: "Archetype",
                table: "Topics",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(24)",
                oldMaxLength: 24);
        }
    }
}
