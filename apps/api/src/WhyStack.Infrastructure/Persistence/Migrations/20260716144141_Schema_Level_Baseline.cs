using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WhyStack.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Schema_Level_Baseline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserLevelBaselines",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    EnteredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLevelBaselines", x => new { x.UserId, x.Level });
                    table.CheckConstraint("CK_UserLevelBaselines_Level", "[Level] IN (10, 20, 30, 40)");
                    table.ForeignKey(
                        name: "FK_UserLevelBaselines_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserLevelBaselines");
        }
    }
}
