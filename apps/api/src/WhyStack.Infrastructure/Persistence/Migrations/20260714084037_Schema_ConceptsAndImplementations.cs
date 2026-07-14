using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WhyStack.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Schema_ConceptsAndImplementations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_TopicSections_TopicVersionId_SectionTypeKey",
                table: "TopicSections");

            migrationBuilder.DropIndex(
                name: "IX_Topics_Technology_DefaultLevel",
                table: "Topics");

            migrationBuilder.DropColumn(
                name: "ContentHash",
                table: "TopicVersions");

            migrationBuilder.DropColumn(
                name: "MarkdownPath",
                table: "TopicVersions");

            migrationBuilder.DropColumn(
                name: "ContentHash",
                table: "TopicTranslations");

            migrationBuilder.DropColumn(
                name: "MarkdownPath",
                table: "TopicTranslations");

            migrationBuilder.DropColumn(
                name: "Technology",
                table: "Topics");

            migrationBuilder.AddColumn<string>(
                name: "Summary",
                table: "TopicTranslations",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "TopicSections",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "LanguageCode",
                table: "TopicSections",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Markdown",
                table: "TopicSections",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "TopicSections",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DomainId",
                table: "Topics",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "IsMandatory",
                table: "SectionTypes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Scope",
                table: "SectionTypes",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Ecosystems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ecosystems", x => x.Id);
                });

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
                name: "Terms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Aliases = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ForbiddenTranslations = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Terms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TopicReviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TopicVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReviewerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromStatus = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    ToStatus = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    Note = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TopicReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TopicReviews_TopicVersions_TopicVersionId",
                        column: x => x.TopicVersionId,
                        principalTable: "TopicVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TopicReviews_Users_ReviewerId",
                        column: x => x.ReviewerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProgrammingLanguages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EcosystemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    FenceLanguage = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgrammingLanguages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgrammingLanguages_Ecosystems_EcosystemId",
                        column: x => x.EcosystemId,
                        principalTable: "Ecosystems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TermExplanations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TermId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LanguageCode = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    Text = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TermExplanations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TermExplanations_Terms_TermId",
                        column: x => x.TermId,
                        principalTable: "Terms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TopicImplementations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TopicVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EcosystemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProgrammingLanguageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SupportedVersions = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TopicImplementations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TopicImplementations_Ecosystems_EcosystemId",
                        column: x => x.EcosystemId,
                        principalTable: "Ecosystems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TopicImplementations_ProgrammingLanguages_ProgrammingLanguageId",
                        column: x => x.ProgrammingLanguageId,
                        principalTable: "ProgrammingLanguages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TopicImplementations_TopicVersions_TopicVersionId",
                        column: x => x.TopicVersionId,
                        principalTable: "TopicVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ImplementationSections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TopicImplementationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SectionTypeKey = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    LanguageCode = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    Markdown = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImplementationSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImplementationSections_SectionTypes_SectionTypeKey",
                        column: x => x.SectionTypeKey,
                        principalTable: "SectionTypes",
                        principalColumn: "Key",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ImplementationSections_TopicImplementations_TopicImplementationId",
                        column: x => x.TopicImplementationId,
                        principalTable: "TopicImplementations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Ecosystems",
                columns: new[] { "Id", "IsAvailable", "Key", "Name", "SortOrder" },
                values: new object[,]
                {
                    { new Guid("3760f81d-ea8c-e777-1e84-ea538c3db9e2"), false, "nodejs", "Node.js", 3 },
                    { new Guid("6c850f94-241d-6842-9f25-56f156664b3b"), false, "java", "Java", 2 },
                    { new Guid("d73a6477-bb02-d027-a6dc-2a4e6a6a8cd3"), true, "dotnet", ".NET", 1 },
                    { new Guid("fc096d90-aa39-9c31-9e65-e3249a824757"), false, "php", "PHP", 4 }
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

            migrationBuilder.UpdateData(
                table: "SectionTypes",
                keyColumn: "Key",
                keyValue: "Alternatives",
                columns: new[] { "IsMandatory", "Scope" },
                values: new object[] { false, "Concept" });

            migrationBuilder.UpdateData(
                table: "SectionTypes",
                keyColumn: "Key",
                keyValue: "ArchitectureContext",
                columns: new[] { "IsMandatory", "Scope" },
                values: new object[] { false, "Concept" });

            migrationBuilder.UpdateData(
                table: "SectionTypes",
                keyColumn: "Key",
                keyValue: "BasicExample",
                columns: new[] { "IsMandatory", "Scope" },
                values: new object[] { false, "Implementation" });

            migrationBuilder.UpdateData(
                table: "SectionTypes",
                keyColumn: "Key",
                keyValue: "BestPractices",
                columns: new[] { "IsMandatory", "Scope" },
                values: new object[] { true, "Concept" });

            migrationBuilder.UpdateData(
                table: "SectionTypes",
                keyColumn: "Key",
                keyValue: "CommonMistakes",
                columns: new[] { "IsMandatory", "Scope" },
                values: new object[] { true, "Concept" });

            migrationBuilder.UpdateData(
                table: "SectionTypes",
                keyColumn: "Key",
                keyValue: "CoreConcepts",
                columns: new[] { "IsMandatory", "Scope" },
                values: new object[] { true, "Concept" });

            migrationBuilder.UpdateData(
                table: "SectionTypes",
                keyColumn: "Key",
                keyValue: "CoreMentalModel",
                columns: new[] { "IsMandatory", "Scope" },
                values: new object[] { true, "Concept" });

            migrationBuilder.UpdateData(
                table: "SectionTypes",
                keyColumn: "Key",
                keyValue: "Definition",
                columns: new[] { "IsMandatory", "Scope" },
                values: new object[] { true, "Concept" });

            migrationBuilder.UpdateData(
                table: "SectionTypes",
                keyColumn: "Key",
                keyValue: "FurtherReading",
                columns: new[] { "IsMandatory", "Scope" },
                values: new object[] { false, "Concept" });

            migrationBuilder.UpdateData(
                table: "SectionTypes",
                keyColumn: "Key",
                keyValue: "Glossary",
                columns: new[] { "IsMandatory", "Scope" },
                values: new object[] { false, "Concept" });

            migrationBuilder.UpdateData(
                table: "SectionTypes",
                keyColumn: "Key",
                keyValue: "HistoricalContext",
                columns: new[] { "IsMandatory", "Scope" },
                values: new object[] { false, "Concept" });

            migrationBuilder.UpdateData(
                table: "SectionTypes",
                keyColumn: "Key",
                keyValue: "InternalMechanics",
                columns: new[] { "IsMandatory", "Scope" },
                values: new object[] { false, "Implementation" });

            migrationBuilder.UpdateData(
                table: "SectionTypes",
                keyColumn: "Key",
                keyValue: "InterviewQuestions",
                columns: new[] { "IsMandatory", "Scope" },
                values: new object[] { false, "Concept" });

            migrationBuilder.UpdateData(
                table: "SectionTypes",
                keyColumn: "Key",
                keyValue: "LearningObjectives",
                columns: new[] { "IsMandatory", "Scope" },
                values: new object[] { true, "Concept" });

            migrationBuilder.UpdateData(
                table: "SectionTypes",
                keyColumn: "Key",
                keyValue: "NextRecommendedTopic",
                columns: new[] { "IsMandatory", "Scope" },
                values: new object[] { false, "Concept" });

            migrationBuilder.UpdateData(
                table: "SectionTypes",
                keyColumn: "Key",
                keyValue: "PerformanceConsiderations",
                columns: new[] { "IsMandatory", "Scope" },
                values: new object[] { false, "Concept" });

            migrationBuilder.UpdateData(
                table: "SectionTypes",
                keyColumn: "Key",
                keyValue: "Prerequisites",
                columns: new[] { "IsMandatory", "Scope" },
                values: new object[] { false, "Concept" });

            migrationBuilder.UpdateData(
                table: "SectionTypes",
                keyColumn: "Key",
                keyValue: "ProblemItSolves",
                columns: new[] { "IsMandatory", "Scope" },
                values: new object[] { true, "Concept" });

            migrationBuilder.UpdateData(
                table: "SectionTypes",
                keyColumn: "Key",
                keyValue: "ProgressiveExamples",
                columns: new[] { "IsMandatory", "Scope" },
                values: new object[] { false, "Implementation" });

            migrationBuilder.UpdateData(
                table: "SectionTypes",
                keyColumn: "Key",
                keyValue: "Quiz",
                columns: new[] { "IsMandatory", "Scope" },
                values: new object[] { false, "Concept" });

            migrationBuilder.UpdateData(
                table: "SectionTypes",
                keyColumn: "Key",
                keyValue: "RealWorldScenario",
                columns: new[] { "IsMandatory", "Scope" },
                values: new object[] { true, "Concept" });

            migrationBuilder.UpdateData(
                table: "SectionTypes",
                keyColumn: "Key",
                keyValue: "RelatedTopics",
                columns: new[] { "IsMandatory", "Scope" },
                values: new object[] { false, "Concept" });

            migrationBuilder.UpdateData(
                table: "SectionTypes",
                keyColumn: "Key",
                keyValue: "SecurityConsiderations",
                columns: new[] { "IsMandatory", "Scope" },
                values: new object[] { false, "Concept" });

            migrationBuilder.UpdateData(
                table: "SectionTypes",
                keyColumn: "Key",
                keyValue: "Summary",
                columns: new[] { "IsMandatory", "Scope" },
                values: new object[] { true, "Concept" });

            migrationBuilder.UpdateData(
                table: "SectionTypes",
                keyColumn: "Key",
                keyValue: "Syntax",
                columns: new[] { "IsMandatory", "Scope" },
                values: new object[] { false, "Implementation" });

            migrationBuilder.UpdateData(
                table: "SectionTypes",
                keyColumn: "Key",
                keyValue: "TestingConsiderations",
                columns: new[] { "IsMandatory", "Scope" },
                values: new object[] { false, "Concept" });

            migrationBuilder.UpdateData(
                table: "SectionTypes",
                keyColumn: "Key",
                keyValue: "TradeOffs",
                columns: new[] { "IsMandatory", "Scope" },
                values: new object[] { true, "Concept" });

            migrationBuilder.UpdateData(
                table: "SectionTypes",
                keyColumn: "Key",
                keyValue: "VersionNotes",
                columns: new[] { "IsMandatory", "Scope" },
                values: new object[] { false, "Implementation" });

            migrationBuilder.UpdateData(
                table: "SectionTypes",
                keyColumn: "Key",
                keyValue: "WhyItExists",
                columns: new[] { "IsMandatory", "Scope" },
                values: new object[] { true, "Concept" });

            migrationBuilder.UpdateData(
                table: "SectionTypes",
                keyColumn: "Key",
                keyValue: "WhyThisTopicMatters",
                columns: new[] { "IsMandatory", "Scope" },
                values: new object[] { true, "Concept" });

            migrationBuilder.InsertData(
                table: "ProgrammingLanguages",
                columns: new[] { "Id", "EcosystemId", "FenceLanguage", "Key", "Name", "SortOrder" },
                values: new object[,]
                {
                    { new Guid("010eea63-f2fe-3d81-314f-39914463faa0"), new Guid("3760f81d-ea8c-e777-1e84-ea538c3db9e2"), "ts", "typescript", "TypeScript", 1 },
                    { new Guid("128dfb6e-1771-c105-c27e-88747e70e54f"), new Guid("d73a6477-bb02-d027-a6dc-2a4e6a6a8cd3"), "csharp", "csharp", "C#", 1 },
                    { new Guid("3d39160c-beaa-0258-c8bf-4bdf1e504ce4"), new Guid("fc096d90-aa39-9c31-9e65-e3249a824757"), "php", "php", "PHP", 1 },
                    { new Guid("61a54f6c-6267-a8b5-41b0-285f50a02567"), new Guid("6c850f94-241d-6842-9f25-56f156664b3b"), "java", "java", "Java", 1 },
                    { new Guid("a0fc966a-22ed-5b5a-e855-fc159eef61f6"), new Guid("d73a6477-bb02-d027-a6dc-2a4e6a6a8cd3"), "fsharp", "fsharp", "F#", 2 },
                    { new Guid("c8d22ab9-aec5-bb09-15aa-0d3cbdd95d6e"), new Guid("6c850f94-241d-6842-9f25-56f156664b3b"), "kotlin", "kotlin", "Kotlin", 2 },
                    { new Guid("d18f51dd-dc11-50c2-9b18-5fc098cd0643"), new Guid("3760f81d-ea8c-e777-1e84-ea538c3db9e2"), "js", "javascript", "JavaScript", 2 }
                });

            migrationBuilder.CreateIndex(
                name: "UX_TopicSections_Version_Type_Language",
                table: "TopicSections",
                columns: new[] { "TopicVersionId", "SectionTypeKey", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Topics_DomainId_DefaultLevel",
                table: "Topics",
                columns: new[] { "DomainId", "DefaultLevel" });

            migrationBuilder.CreateIndex(
                name: "UX_Ecosystems_Key",
                table: "Ecosystems",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImplementationSections_SectionTypeKey",
                table: "ImplementationSections",
                column: "SectionTypeKey");

            migrationBuilder.CreateIndex(
                name: "UX_ImplementationSections_Impl_Type_Language",
                table: "ImplementationSections",
                columns: new[] { "TopicImplementationId", "SectionTypeKey", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_KnowledgeDomains_Key",
                table: "KnowledgeDomains",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProgrammingLanguages_EcosystemId",
                table: "ProgrammingLanguages",
                column: "EcosystemId");

            migrationBuilder.CreateIndex(
                name: "UX_ProgrammingLanguages_Key",
                table: "ProgrammingLanguages",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_TermExplanations_TermId_LanguageCode",
                table: "TermExplanations",
                columns: new[] { "TermId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Terms_Text",
                table: "Terms",
                column: "Text",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TopicImplementations_EcosystemId",
                table: "TopicImplementations",
                column: "EcosystemId");

            migrationBuilder.CreateIndex(
                name: "IX_TopicImplementations_ProgrammingLanguageId",
                table: "TopicImplementations",
                column: "ProgrammingLanguageId");

            migrationBuilder.CreateIndex(
                name: "UX_TopicImplementations_TopicVersionId_EcosystemId",
                table: "TopicImplementations",
                columns: new[] { "TopicVersionId", "EcosystemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TopicReviews_ReviewerId",
                table: "TopicReviews",
                column: "ReviewerId");

            migrationBuilder.CreateIndex(
                name: "IX_TopicReviews_TopicVersionId",
                table: "TopicReviews",
                column: "TopicVersionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Topics_KnowledgeDomains_DomainId",
                table: "Topics",
                column: "DomainId",
                principalTable: "KnowledgeDomains",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Topics_KnowledgeDomains_DomainId",
                table: "Topics");

            migrationBuilder.DropTable(
                name: "ImplementationSections");

            migrationBuilder.DropTable(
                name: "KnowledgeDomains");

            migrationBuilder.DropTable(
                name: "TermExplanations");

            migrationBuilder.DropTable(
                name: "TopicReviews");

            migrationBuilder.DropTable(
                name: "TopicImplementations");

            migrationBuilder.DropTable(
                name: "Terms");

            migrationBuilder.DropTable(
                name: "ProgrammingLanguages");

            migrationBuilder.DropTable(
                name: "Ecosystems");

            migrationBuilder.DropIndex(
                name: "UX_TopicSections_Version_Type_Language",
                table: "TopicSections");

            migrationBuilder.DropIndex(
                name: "IX_Topics_DomainId_DefaultLevel",
                table: "Topics");

            migrationBuilder.DropColumn(
                name: "Summary",
                table: "TopicTranslations");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "TopicSections");

            migrationBuilder.DropColumn(
                name: "LanguageCode",
                table: "TopicSections");

            migrationBuilder.DropColumn(
                name: "Markdown",
                table: "TopicSections");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "TopicSections");

            migrationBuilder.DropColumn(
                name: "DomainId",
                table: "Topics");

            migrationBuilder.DropColumn(
                name: "IsMandatory",
                table: "SectionTypes");

            migrationBuilder.DropColumn(
                name: "Scope",
                table: "SectionTypes");

            migrationBuilder.AddColumn<string>(
                name: "ContentHash",
                table: "TopicVersions",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MarkdownPath",
                table: "TopicVersions",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContentHash",
                table: "TopicTranslations",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MarkdownPath",
                table: "TopicTranslations",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Technology",
                table: "Topics",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "UX_TopicSections_TopicVersionId_SectionTypeKey",
                table: "TopicSections",
                columns: new[] { "TopicVersionId", "SectionTypeKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Topics_Technology_DefaultLevel",
                table: "Topics",
                columns: new[] { "Technology", "DefaultLevel" });
        }
    }
}
