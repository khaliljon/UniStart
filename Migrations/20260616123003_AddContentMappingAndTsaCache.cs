using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UniStart.Migrations
{
    /// <inheritdoc />
    public partial class AddContentMappingAndTsaCache : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContentMappingRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExamSectionName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    MatchType = table.Column<int>(type: "integer", nullable: false),
                    Pattern = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    SkillName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Glossary = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentMappingRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TsaClassifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QuestionHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SkillName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    TopicName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    ExamSectionName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    QuestionPreview = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastSeenAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TsaClassifications", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContentMappingRules_ExamSectionName_IsActive_SortOrder",
                table: "ContentMappingRules",
                columns: new[] { "ExamSectionName", "IsActive", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_TsaClassifications_QuestionHash",
                table: "TsaClassifications",
                column: "QuestionHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContentMappingRules");

            migrationBuilder.DropTable(
                name: "TsaClassifications");
        }
    }
}
