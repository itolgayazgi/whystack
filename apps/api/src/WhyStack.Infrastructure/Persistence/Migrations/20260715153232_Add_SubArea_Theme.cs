using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WhyStack.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Add_SubArea_Theme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SubAreaId",
                table: "Topics",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SubAreas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubAreas", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "SubAreas",
                columns: new[] { "Id", "Key", "Name", "SortOrder" },
                values: new object[,]
                {
                    { new Guid("2d97c76f-2856-c65c-c93b-f5242a330196"), "dependency-injection", "Dependency Injection", 5 },
                    { new Guid("561d4f29-1a71-6548-7959-17d7fbff9bbb"), "async", "Async / Await", 1 },
                    { new Guid("b8c01adc-9a89-7d1f-c664-04c6f4b9b0c2"), "collections", "Koleksiyonlar", 3 },
                    { new Guid("c1a104fa-41f9-69b9-8219-e33937fe5648"), "error-handling", "Hata Yönetimi", 4 },
                    { new Guid("c92cdedb-7d89-b251-5546-f3cd387e1be1"), "concurrency", "Eşzamanlılık", 6 },
                    { new Guid("fde9c505-0ab1-4f6f-8923-b4044e0c3f93"), "memory-management", "Bellek Yönetimi", 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Topics_SubAreaId_DefaultLevel",
                table: "Topics",
                columns: new[] { "SubAreaId", "DefaultLevel" },
                filter: "[SubAreaId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_SubAreas_Key",
                table: "SubAreas",
                column: "Key",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Topics_SubAreas_SubAreaId",
                table: "Topics",
                column: "SubAreaId",
                principalTable: "SubAreas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Topics_SubAreas_SubAreaId",
                table: "Topics");

            migrationBuilder.DropTable(
                name: "SubAreas");

            migrationBuilder.DropIndex(
                name: "IX_Topics_SubAreaId_DefaultLevel",
                table: "Topics");

            migrationBuilder.DropColumn(
                name: "SubAreaId",
                table: "Topics");
        }
    }
}
