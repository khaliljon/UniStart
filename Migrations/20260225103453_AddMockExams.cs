using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UniStart.Migrations
{
    /// <inheritdoc />
    public partial class AddMockExams : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReadingPassageId",
                table: "Questions",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MockExams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExamTypeCode = table.Column<string>(type: "character varying(10)", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    TotalTimeMinutes = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MockExams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MockExams_ExamTypes_ExamTypeCode",
                        column: x => x.ExamTypeCode,
                        principalTable: "ExamTypes",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReadingPassages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TopicId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReadingPassages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReadingPassages_Topics_TopicId",
                        column: x => x.TopicId,
                        principalTable: "Topics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MockExamAttempts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    MockExamId = table.Column<int>(type: "integer", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CurrentSectionIndex = table.Column<int>(type: "integer", nullable: false),
                    TotalScore = table.Column<double>(type: "double precision", nullable: true),
                    SectionScoresJson = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MockExamAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MockExamAttempts_MockExams_MockExamId",
                        column: x => x.MockExamId,
                        principalTable: "MockExams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MockExamAttempts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MockExamSections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MockExamId = table.Column<int>(type: "integer", nullable: false),
                    ExamSectionId = table.Column<int>(type: "integer", nullable: true),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TimeLimitMinutes = table.Column<int>(type: "integer", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    Instructions = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MockExamSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MockExamSections_ExamSections_ExamSectionId",
                        column: x => x.ExamSectionId,
                        principalTable: "ExamSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MockExamSections_MockExams_MockExamId",
                        column: x => x.MockExamId,
                        principalTable: "MockExams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MockExamAnswers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AttemptId = table.Column<int>(type: "integer", nullable: false),
                    QuestionId = table.Column<int>(type: "integer", nullable: false),
                    SelectedOptionId = table.Column<int>(type: "integer", nullable: true),
                    SectionIndex = table.Column<int>(type: "integer", nullable: false),
                    TimeSpentSeconds = table.Column<int>(type: "integer", nullable: true),
                    IsCorrect = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MockExamAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MockExamAnswers_AnswerOptions_SelectedOptionId",
                        column: x => x.SelectedOptionId,
                        principalTable: "AnswerOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MockExamAnswers_MockExamAttempts_AttemptId",
                        column: x => x.AttemptId,
                        principalTable: "MockExamAttempts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MockExamAnswers_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Questions_ReadingPassageId",
                table: "Questions",
                column: "ReadingPassageId");

            migrationBuilder.CreateIndex(
                name: "IX_MockExamAnswers_AttemptId",
                table: "MockExamAnswers",
                column: "AttemptId");

            migrationBuilder.CreateIndex(
                name: "IX_MockExamAnswers_QuestionId",
                table: "MockExamAnswers",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_MockExamAnswers_SelectedOptionId",
                table: "MockExamAnswers",
                column: "SelectedOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_MockExamAttempts_MockExamId",
                table: "MockExamAttempts",
                column: "MockExamId");

            migrationBuilder.CreateIndex(
                name: "IX_MockExamAttempts_UserId",
                table: "MockExamAttempts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MockExams_ExamTypeCode",
                table: "MockExams",
                column: "ExamTypeCode");

            migrationBuilder.CreateIndex(
                name: "IX_MockExamSections_ExamSectionId",
                table: "MockExamSections",
                column: "ExamSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_MockExamSections_MockExamId",
                table: "MockExamSections",
                column: "MockExamId");

            migrationBuilder.CreateIndex(
                name: "IX_ReadingPassages_TopicId",
                table: "ReadingPassages",
                column: "TopicId");

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_ReadingPassages_ReadingPassageId",
                table: "Questions",
                column: "ReadingPassageId",
                principalTable: "ReadingPassages",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Questions_ReadingPassages_ReadingPassageId",
                table: "Questions");

            migrationBuilder.DropTable(
                name: "MockExamAnswers");

            migrationBuilder.DropTable(
                name: "MockExamSections");

            migrationBuilder.DropTable(
                name: "ReadingPassages");

            migrationBuilder.DropTable(
                name: "MockExamAttempts");

            migrationBuilder.DropTable(
                name: "MockExams");

            migrationBuilder.DropIndex(
                name: "IX_Questions_ReadingPassageId",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "ReadingPassageId",
                table: "Questions");
        }
    }
}
