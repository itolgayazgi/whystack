using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WhyStack.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Schema_Ecosystem_Area : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The AddColumn below fills every existing row with Guid.Empty — EF has nothing else to put
            // there — and the UpdateData calls after it only fix the four SEEDED keys. Any ecosystem row
            // somebody added by hand would keep Guid.Empty, and the foreign key at the end would fail with
            // a constraint error that names the FK and not the cause.
            //
            // So it fails HERE, saying what is actually wrong, before touching anything.
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM [Ecosystems] WHERE [Key] NOT IN ('dotnet','java','nodejs','php'))
                    THROW 50000, 'An ecosystem exists that this migration does not know which area to put it in. Add it to the seed or give it an area first. Nothing was changed.', 1;
                """);

            migrationBuilder.DropIndex(
                name: "UX_Ecosystems_Key",
                table: "Ecosystems");

            migrationBuilder.AddColumn<Guid>(
                name: "AreaId",
                table: "Ecosystems",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.UpdateData(
                table: "Ecosystems",
                keyColumn: "Id",
                keyValue: new Guid("3760f81d-ea8c-e777-1e84-ea538c3db9e2"),
                column: "AreaId",
                value: new Guid("6f230013-272c-3529-1909-f1497a729d02"));

            migrationBuilder.UpdateData(
                table: "Ecosystems",
                keyColumn: "Id",
                keyValue: new Guid("6c850f94-241d-6842-9f25-56f156664b3b"),
                column: "AreaId",
                value: new Guid("6f230013-272c-3529-1909-f1497a729d02"));

            migrationBuilder.UpdateData(
                table: "Ecosystems",
                keyColumn: "Id",
                keyValue: new Guid("d73a6477-bb02-d027-a6dc-2a4e6a6a8cd3"),
                column: "AreaId",
                value: new Guid("6f230013-272c-3529-1909-f1497a729d02"));

            migrationBuilder.UpdateData(
                table: "Ecosystems",
                keyColumn: "Id",
                keyValue: new Guid("fc096d90-aa39-9c31-9e65-e3249a824757"),
                column: "AreaId",
                value: new Guid("6f230013-272c-3529-1909-f1497a729d02"));

            migrationBuilder.CreateIndex(
                name: "UX_Ecosystems_AreaId_Key",
                table: "Ecosystems",
                columns: new[] { "AreaId", "Key" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Ecosystems_Areas_AreaId",
                table: "Ecosystems",
                column: "AreaId",
                principalTable: "Areas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ecosystems_Areas_AreaId",
                table: "Ecosystems");

            migrationBuilder.DropIndex(
                name: "UX_Ecosystems_AreaId_Key",
                table: "Ecosystems");

            migrationBuilder.DropColumn(
                name: "AreaId",
                table: "Ecosystems");

            migrationBuilder.CreateIndex(
                name: "UX_Ecosystems_Key",
                table: "Ecosystems",
                column: "Key",
                unique: true);
        }
    }
}
