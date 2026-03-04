using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UniStart.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestionImport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QuestionImportJobs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AdminUserId = table.Column<int>(type: "integer", nullable: false),
                    FileName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FileType = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    ExamTypeCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SectionId = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TotalExtracted = table.Column<int>(type: "integer", nullable: false),
                    TotalApproved = table.Column<int>(type: "integer", nullable: false),
                    TotalRejected = table.Column<int>(type: "integer", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionImportJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionImportJobs_Users_AdminUserId",
                        column: x => x.AdminUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ImportedQuestionDrafts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ImportJobId = table.Column<int>(type: "integer", nullable: false),
                    QuestionText = table.Column<string>(type: "text", nullable: false),
                    OptionsJson = table.Column<string>(type: "text", nullable: false, defaultValue: "[]"),
                    Explanation = table.Column<string>(type: "text", nullable: true),
                    Hint = table.Column<string>(type: "text", nullable: true),
                    TopicId = table.Column<int>(type: "integer", nullable: true),
                    Difficulty = table.Column<int>(type: "integer", nullable: false),
                    IrtA = table.Column<double>(type: "double precision", nullable: false),
                    IrtB = table.Column<double>(type: "double precision", nullable: false),
                    IrtC = table.Column<double>(type: "double precision", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Source = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewedByUserId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportedQuestionDrafts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImportedQuestionDrafts_QuestionImportJobs_ImportJobId",
                        column: x => x.ImportJobId,
                        principalTable: "QuestionImportJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ImportedQuestionDrafts_Topics_TopicId",
                        column: x => x.TopicId,
                        principalTable: "Topics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ImportedQuestionDrafts_Job_Status",
                table: "ImportedQuestionDrafts",
                columns: new[] { "ImportJobId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ImportedQuestionDrafts_TopicId",
                table: "ImportedQuestionDrafts",
                column: "TopicId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionImportJobs_AdminUserId",
                table: "QuestionImportJobs",
                column: "AdminUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImportedQuestionDrafts");

            migrationBuilder.DropTable(
                name: "QuestionImportJobs");
        }
    }
}
