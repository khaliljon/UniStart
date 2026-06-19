using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UniStart.Migrations
{
    /// <inheritdoc />
    public partial class RemoveContentPipelineModules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContentMappingRules");

            migrationBuilder.DropTable(
                name: "DriveSyncItems");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContentMappingRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExamSectionName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Glossary = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsIgnore = table.Column<bool>(type: "boolean", nullable: false),
                    MatchType = table.Column<int>(type: "integer", nullable: false),
                    Pattern = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    SkillName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentMappingRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DriveSyncItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Checksum = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DriveFileId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    DriveModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    FolderPath = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    LastResultJson = table.Column<string>(type: "text", nullable: true),
                    LastSyncedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MappedSkillId = table.Column<int>(type: "integer", nullable: true),
                    MappedSkillName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    MimeType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriveSyncItems", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContentMappingRules_ExamSectionName_IsActive_SortOrder",
                table: "ContentMappingRules",
                columns: new[] { "ExamSectionName", "IsActive", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_DriveSyncItems_DriveFileId",
                table: "DriveSyncItems",
                column: "DriveFileId",
                unique: true);
        }
    }
}
