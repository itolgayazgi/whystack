using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WhyStack.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Schema_Content : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SectionTypes",
                columns: table => new
                {
                    Key = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsGraphDerived = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SectionTypes", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "Topics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StableKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Technology = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    DefaultLevel = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    DefaultTitle = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Topics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TopicRelationships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromTopicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ToTopicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TopicRelationships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TopicRelationships_Topics_FromTopicId",
                        column: x => x.FromTopicId,
                        principalTable: "Topics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TopicRelationships_Topics_ToTopicId",
                        column: x => x.ToTopicId,
                        principalTable: "Topics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TopicVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TopicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VersionNumber = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    CanonicalLanguageCode = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    MarkdownPath = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    ContentHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    EstimatedReadingMinutes = table.Column<int>(type: "int", nullable: false),
                    LastReviewedOn = table.Column<DateOnly>(type: "date", nullable: false),
                    PublishedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeprecatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TopicVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TopicVersions_Topics_TopicId",
                        column: x => x.TopicId,
                        principalTable: "Topics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TopicSections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TopicVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SectionTypeKey = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TopicSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TopicSections_SectionTypes_SectionTypeKey",
                        column: x => x.SectionTypeKey,
                        principalTable: "SectionTypes",
                        principalColumn: "Key",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TopicSections_TopicVersions_TopicVersionId",
                        column: x => x.TopicVersionId,
                        principalTable: "TopicVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TopicSupportedVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TopicVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Version = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TopicSupportedVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TopicSupportedVersions_TopicVersions_TopicVersionId",
                        column: x => x.TopicVersionId,
                        principalTable: "TopicVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TopicTranslations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TopicVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LanguageCode = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    MarkdownPath = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    ContentHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TopicTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TopicTranslations_TopicVersions_TopicVersionId",
                        column: x => x.TopicVersionId,
                        principalTable: "TopicVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "SectionTypes",
                columns: new[] { "Key", "IsGraphDerived", "SortOrder" },
                values: new object[,]
                {
                    { "Alternatives", false, 23 },
                    { "ArchitectureContext", false, 16 },
                    { "BasicExample", false, 13 },
                    { "BestPractices", false, 20 },
                    { "CommonMistakes", false, 21 },
                    { "CoreConcepts", false, 10 },
                    { "CoreMentalModel", false, 9 },
                    { "Definition", false, 5 },
                    { "FurtherReading", false, 29 },
                    { "Glossary", true, 30 },
                    { "HistoricalContext", false, 8 },
                    { "InternalMechanics", false, 11 },
                    { "InterviewQuestions", false, 25 },
                    { "LearningObjectives", false, 2 },
                    { "NextRecommendedTopic", true, 28 },
                    { "PerformanceConsiderations", false, 17 },
                    { "Prerequisites", true, 4 },
                    { "ProblemItSolves", false, 7 },
                    { "ProgressiveExamples", false, 14 },
                    { "Quiz", false, 26 },
                    { "RealWorldScenario", false, 15 },
                    { "RelatedTopics", true, 27 },
                    { "SecurityConsiderations", false, 18 },
                    { "Summary", false, 1 },
                    { "Syntax", false, 12 },
                    { "TestingConsiderations", false, 19 },
                    { "TradeOffs", false, 22 },
                    { "VersionNotes", false, 24 },
                    { "WhyItExists", false, 6 },
                    { "WhyThisTopicMatters", false, 3 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_TopicRelationships_ToTopicId",
                table: "TopicRelationships",
                column: "ToTopicId");

            migrationBuilder.CreateIndex(
                name: "UX_TopicRelationships_From_To_Type",
                table: "TopicRelationships",
                columns: new[] { "FromTopicId", "ToTopicId", "Type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Topics_Technology_DefaultLevel",
                table: "Topics",
                columns: new[] { "Technology", "DefaultLevel" });

            migrationBuilder.CreateIndex(
                name: "UX_Topics_Slug",
                table: "Topics",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Topics_StableKey",
                table: "Topics",
                column: "StableKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TopicSections_SectionTypeKey",
                table: "TopicSections",
                column: "SectionTypeKey");

            migrationBuilder.CreateIndex(
                name: "UX_TopicSections_TopicVersionId_SectionTypeKey",
                table: "TopicSections",
                columns: new[] { "TopicVersionId", "SectionTypeKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_TopicSupportedVersions_TopicVersionId_Version",
                table: "TopicSupportedVersions",
                columns: new[] { "TopicVersionId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_TopicTranslations_TopicVersionId_LanguageCode",
                table: "TopicTranslations",
                columns: new[] { "TopicVersionId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_TopicVersions_TopicId_VersionNumber",
                table: "TopicVersions",
                columns: new[] { "TopicId", "VersionNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TopicRelationships");

            migrationBuilder.DropTable(
                name: "TopicSections");

            migrationBuilder.DropTable(
                name: "TopicSupportedVersions");

            migrationBuilder.DropTable(
                name: "TopicTranslations");

            migrationBuilder.DropTable(
                name: "SectionTypes");

            migrationBuilder.DropTable(
                name: "TopicVersions");

            migrationBuilder.DropTable(
                name: "Topics");
        }
    }
}
