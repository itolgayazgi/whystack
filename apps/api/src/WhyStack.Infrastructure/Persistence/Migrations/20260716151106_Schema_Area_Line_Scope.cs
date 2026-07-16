using System;
using Microsoft.EntityFrameworkCore.Migrations;
using WhyStack.Infrastructure.Persistence;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WhyStack.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Schema_Area_Line_Scope : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── The data step EF cannot scaffold ──────────────────────────────────────────────────
            //
            // The rename below turns DomainId into LineId, and the old value is a KnowledgeDomains id while
            // the new table holds Lines ids. Left alone, every existing topic points at a line that does not
            // exist and the foreign key fails on the first row.
            //
            // Half of the old domains map exactly, because they were lines wearing the wrong name
            // (ADR-0027): language IS B1, architecture IS B4. The other half — `backend`, `database` — were
            // AREAS, and an area has eight lines with nothing to say which one a topic belongs on. That is
            // not a mapping problem to be solved with a default; it is a question only a human can answer.
            //
            // So this THROWS rather than parking them somewhere plausible. A migration that guesses puts
            // every unmappable topic on B1 and tells nobody — and a wrong line is not a wrong label, it is a
            // topic on the wrong map, findable by nobody who is looking for it.
            migrationBuilder.Sql("""
                IF OBJECT_ID('dbo.Topics', 'U') IS NOT NULL AND EXISTS (
                    SELECT 1 FROM [Topics] t JOIN [KnowledgeDomains] d ON d.Id = t.DomainId
                    WHERE d.[Key] NOT IN ('language', 'architecture', 'security', 'testing'))
                    THROW 50000, 'Some topics sit on a domain that was an AREA, not a line (backend, database...). An area has eight lines and nothing says which one they belong on — re-point them at a line first. Nothing was changed.', 1;
                """);

            migrationBuilder.Sql("""
                IF OBJECT_ID('dbo.Topics', 'U') IS NOT NULL AND EXISTS (
                    SELECT 1 FROM [Topics] t JOIN [SubAreas] a ON a.Id = t.SubAreaId
                    WHERE a.[Key] NOT IN ('async','memory-management','collections','error-handling','concurrency','dependency-injection'))
                    THROW 50000, 'A topic is tagged with a scope this migration cannot re-parent onto a line. Add it to the mapping or untag the topic first. Nothing was changed.', 1;
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_Topics_KnowledgeDomains_DomainId",
                table: "Topics");

            migrationBuilder.DropForeignKey(
                name: "FK_Topics_SubAreas_SubAreaId",
                table: "Topics");

            migrationBuilder.DropTable(
                name: "KnowledgeDomains");

            migrationBuilder.DropTable(
                name: "SubAreas");

            migrationBuilder.DropIndex(
                name: "IX_Topics_SubAreaId_DefaultLevel",
                table: "Topics");

            migrationBuilder.RenameColumn(
                name: "SubAreaId",
                table: "Topics",
                newName: "ScopeId");

            migrationBuilder.RenameColumn(
                name: "DomainId",
                table: "Topics",
                newName: "LineId");

            // The exact half: these four domains WERE lines, under a name that also had to serve as an area.
            // DeterministicId.For("line:b1-language-runtime") and friends are what the seed above inserted,
            // so the ids here are not magic numbers — they are the same function, evaluated.
            migrationBuilder.Sql($"""
                UPDATE [Topics] SET [LineId] = CASE CAST([LineId] AS uniqueidentifier)
                    WHEN '{DeterministicId.For("domain:language")}'     THEN '{DeterministicId.For("line:b1-language-runtime")}'
                    WHEN '{DeterministicId.For("domain:architecture")}' THEN '{DeterministicId.For("line:b4-architecture")}'
                    WHEN '{DeterministicId.For("domain:security")}'     THEN '{DeterministicId.For("line:b6-security")}'
                    WHEN '{DeterministicId.For("domain:testing")}'      THEN '{DeterministicId.For("line:b7-testing")}'
                    ELSE [LineId] END;
                """);

            // The same for the scopes. Their ids changed shape, not just table: a scope's key is now unique
            // per LINE (ADR-0027), so the id is derived from "scope:{line}:{key}" rather than "subarea:{key}"
            // — which is what lets B1's "Eşzamanlılık" and B3's "Transaction & Eşzamanlılık" coexist.
            //
            // The seeds are re-parented, not re-cut: these are ADR-0023's themes, which were never wrong,
            // only homeless.
            migrationBuilder.Sql($"""
                UPDATE [Topics] SET [ScopeId] = CASE CAST([ScopeId] AS uniqueidentifier)
                    WHEN '{DeterministicId.For("subarea:async")}'                THEN '{DeterministicId.For("scope:b1-language-runtime:async")}'
                    WHEN '{DeterministicId.For("subarea:memory-management")}'    THEN '{DeterministicId.For("scope:b1-language-runtime:memory-management")}'
                    WHEN '{DeterministicId.For("subarea:collections")}'          THEN '{DeterministicId.For("scope:b1-language-runtime:collections")}'
                    WHEN '{DeterministicId.For("subarea:error-handling")}'       THEN '{DeterministicId.For("scope:b1-language-runtime:error-handling")}'
                    WHEN '{DeterministicId.For("subarea:concurrency")}'          THEN '{DeterministicId.For("scope:b1-language-runtime:concurrency")}'
                    WHEN '{DeterministicId.For("subarea:dependency-injection")}' THEN '{DeterministicId.For("scope:b4-architecture:dependency-injection")}'
                    ELSE [ScopeId] END
                WHERE [ScopeId] IS NOT NULL;
                """);


            migrationBuilder.RenameIndex(
                name: "IX_Topics_DomainId_DefaultLevel",
                table: "Topics",
                newName: "IX_Topics_LineId_DefaultLevel");

            migrationBuilder.AddColumn<string>(
                name: "SequenceGroup",
                table: "Topics",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SequenceOf",
                table: "Topics",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SequencePart",
                table: "Topics",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Areas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Areas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Lines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    AreaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lines_Areas_AreaId",
                        column: x => x.AreaId,
                        principalTable: "Areas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Scopes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    LineId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scopes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Scopes_Lines_LineId",
                        column: x => x.LineId,
                        principalTable: "Lines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Areas",
                columns: new[] { "Id", "Key", "Name", "SortOrder" },
                values: new object[,]
                {
                    { new Guid("3d340a97-20ef-1916-3890-6168e7283f70"), "database", "Database", 3 },
                    { new Guid("6f230013-272c-3529-1909-f1497a729d02"), "backend", "Backend", 1 },
                    { new Guid("a899d836-2ba0-d477-8b22-c1c7e43a31df"), "devops", "DevOps", 4 },
                    { new Guid("fa42a338-df9d-fb7b-4959-5f97ea115211"), "frontend", "Frontend", 2 }
                });

            migrationBuilder.InsertData(
                table: "Lines",
                columns: new[] { "Id", "AreaId", "Color", "Key", "Name", "SortOrder" },
                values: new object[,]
                {
                    { new Guid("0209cd3e-4523-5c9e-26e2-f81e6da03f96"), new Guid("6f230013-272c-3529-1909-f1497a729d02"), "#6C9BD1", "b3-data-access", "Veri Erişimi", 3 },
                    { new Guid("202dc30e-c895-c9de-74c4-4a18a0f805e8"), new Guid("6f230013-272c-3529-1909-f1497a729d02"), "#D96A5F", "b6-security", "Güvenlik & Kimlik", 6 },
                    { new Guid("209c6ac2-2a17-1abc-8196-7a9c1c365ab7"), new Guid("6f230013-272c-3529-1909-f1497a729d02"), "#C9A227", "b1-language-runtime", "Dil & Runtime", 1 },
                    { new Guid("932e7817-b8f6-8266-73da-8e236978e430"), new Guid("6f230013-272c-3529-1909-f1497a729d02"), "#8FBF9F", "b5-messaging", "Mesajlaşma & Dağıtık", 5 },
                    { new Guid("acf8d7d7-96e3-d2c9-4f73-7e4570c0787e"), new Guid("6f230013-272c-3529-1909-f1497a729d02"), "#5BB8C4", "b7-testing", "Test & Kalite", 7 },
                    { new Guid("c0bf2be9-8d21-c6c9-f41d-df09bba98fd6"), new Guid("6f230013-272c-3529-1909-f1497a729d02"), "#C98A5A", "b2-web-api", "Web API & Framework", 2 },
                    { new Guid("e4e4a1dc-4457-8f8d-7583-0e87206ffec5"), new Guid("6f230013-272c-3529-1909-f1497a729d02"), "#A98BC9", "b4-architecture", "Mimari & Tasarım", 4 },
                    { new Guid("eed4bba5-7873-a134-8296-35ce2aa04230"), new Guid("6f230013-272c-3529-1909-f1497a729d02"), "#B07A4A", "b8-performance", "Performans & Gözlemlenebilirlik", 8 }
                });

            migrationBuilder.InsertData(
                table: "Scopes",
                columns: new[] { "Id", "Key", "LineId", "Name", "SortOrder" },
                values: new object[,]
                {
                    { new Guid("03470a92-b4b7-5165-4653-7de7b339fca4"), "async", new Guid("209c6ac2-2a17-1abc-8196-7a9c1c365ab7"), "Async / Await", 1 },
                    { new Guid("3a7eb6b5-62d2-bc3c-d5fb-dc6342553c0d"), "dependency-injection", new Guid("e4e4a1dc-4457-8f8d-7583-0e87206ffec5"), "Dependency Injection", 1 },
                    { new Guid("47c5fd71-ff1e-eab2-cb11-73bf18195b64"), "memory-management", new Guid("209c6ac2-2a17-1abc-8196-7a9c1c365ab7"), "Bellek Yönetimi", 2 },
                    { new Guid("4a9ef775-c8e9-382f-2bcd-4cdc2c103e6b"), "concurrency", new Guid("209c6ac2-2a17-1abc-8196-7a9c1c365ab7"), "Eşzamanlılık", 5 },
                    { new Guid("8d05a1fa-7053-bc16-f08c-24e8eb91b509"), "error-handling", new Guid("209c6ac2-2a17-1abc-8196-7a9c1c365ab7"), "Hata Yönetimi", 4 },
                    { new Guid("c3c60eac-5484-97fb-07f7-9bb1bb6e9f7f"), "collections", new Guid("209c6ac2-2a17-1abc-8196-7a9c1c365ab7"), "Koleksiyonlar", 3 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Topics_ScopeId_DefaultLevel",
                table: "Topics",
                columns: new[] { "ScopeId", "DefaultLevel" },
                filter: "[ScopeId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_Areas_Key",
                table: "Areas",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lines_AreaId",
                table: "Lines",
                column: "AreaId");

            migrationBuilder.CreateIndex(
                name: "UX_Lines_Key",
                table: "Lines",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Scopes_LineId_Key",
                table: "Scopes",
                columns: new[] { "LineId", "Key" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Topics_Lines_LineId",
                table: "Topics",
                column: "LineId",
                principalTable: "Lines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Topics_Scopes_ScopeId",
                table: "Topics",
                column: "ScopeId",
                principalTable: "Scopes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Topics_Lines_LineId",
                table: "Topics");

            migrationBuilder.DropForeignKey(
                name: "FK_Topics_Scopes_ScopeId",
                table: "Topics");

            migrationBuilder.DropTable(
                name: "Scopes");

            migrationBuilder.DropTable(
                name: "Lines");

            migrationBuilder.DropTable(
                name: "Areas");

            migrationBuilder.DropIndex(
                name: "IX_Topics_ScopeId_DefaultLevel",
                table: "Topics");

            migrationBuilder.DropColumn(
                name: "SequenceGroup",
                table: "Topics");

            migrationBuilder.DropColumn(
                name: "SequenceOf",
                table: "Topics");

            migrationBuilder.DropColumn(
                name: "SequencePart",
                table: "Topics");

            migrationBuilder.RenameColumn(
                name: "ScopeId",
                table: "Topics",
                newName: "SubAreaId");

            migrationBuilder.RenameColumn(
                name: "LineId",
                table: "Topics",
                newName: "DomainId");

            migrationBuilder.RenameIndex(
                name: "IX_Topics_LineId_DefaultLevel",
                table: "Topics",
                newName: "IX_Topics_DomainId_DefaultLevel");

            migrationBuilder.CreateTable(
                name: "KnowledgeDomains",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KnowledgeDomains", x => x.Id);
                });

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
                table: "KnowledgeDomains",
                columns: new[] { "Id", "Key", "Name", "SortOrder" },
                values: new object[,]
                {
                    { new Guid("2e6c62e8-a477-c23e-727a-91dd900d508f"), "database", "Database", 2 },
                    { new Guid("41d1d484-6405-0257-9a57-09ca5202b61f"), "devops", "DevOps", 6 },
                    { new Guid("4f8a0a68-63fa-4dcd-3484-78fb4b313db1"), "language", "Language", 3 },
                    { new Guid("58c93303-459d-b159-502a-459be534f867"), "backend", "Backend", 1 },
                    { new Guid("a57963e8-a284-9bf6-a395-515b9433e05b"), "architecture", "Architecture", 4 },
                    { new Guid("b8273c4a-efd0-6ad5-cdf1-10e32e043cde"), "networking", "Networking", 5 },
                    { new Guid("bca126a8-66cc-f22b-336c-55b8874036d4"), "testing", "Testing", 8 },
                    { new Guid("ff40d5ba-3651-4265-b3e4-18e58f3315c5"), "security", "Security", 7 }
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
                name: "UX_KnowledgeDomains_Key",
                table: "KnowledgeDomains",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_SubAreas_Key",
                table: "SubAreas",
                column: "Key",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Topics_KnowledgeDomains_DomainId",
                table: "Topics",
                column: "DomainId",
                principalTable: "KnowledgeDomains",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Topics_SubAreas_SubAreaId",
                table: "Topics",
                column: "SubAreaId",
                principalTable: "SubAreas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
