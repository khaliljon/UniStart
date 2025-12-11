using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UniStart.Migrations
{
    /// <inheritdoc />
    public partial class AddUserFlashcardProgressAndUserQuizAnswer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Flashcards_NextReviewDate",
                table: "Flashcards");

            migrationBuilder.AlterColumn<string>(
                name: "UserAnswersJson",
                table: "UserQuizAttempts",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateTable(
                name: "UserFlashcardProgresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    FlashcardId = table.Column<int>(type: "integer", nullable: false),
                    EaseFactor = table.Column<double>(type: "double precision", nullable: false),
                    Interval = table.Column<int>(type: "integer", nullable: false),
                    Repetitions = table.Column<int>(type: "integer", nullable: false),
                    NextReviewDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TotalReviews = table.Column<int>(type: "integer", nullable: false),
                    CorrectReviews = table.Column<int>(type: "integer", nullable: false),
                    FirstReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsMastered = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFlashcardProgresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserFlashcardProgresses_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserFlashcardProgresses_Flashcards_FlashcardId",
                        column: x => x.FlashcardId,
                        principalTable: "Flashcards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserFlashcardSetAccesses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    FlashcardSetId = table.Column<int>(type: "integer", nullable: false),
                    FirstAccessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastAccessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AccessCount = table.Column<int>(type: "integer", nullable: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CardsStudiedCount = table.Column<int>(type: "integer", nullable: false),
                    TotalCardsCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFlashcardSetAccesses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserFlashcardSetAccesses_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserFlashcardSetAccesses_FlashcardSets_FlashcardSetId",
                        column: x => x.FlashcardSetId,
                        principalTable: "FlashcardSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserQuizAnswers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IsCorrect = table.Column<bool>(type: "boolean", nullable: false),
                    PointsEarned = table.Column<int>(type: "integer", nullable: false),
                    AnsweredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AttemptId = table.Column<int>(type: "integer", nullable: false),
                    QuestionId = table.Column<int>(type: "integer", nullable: false),
                    SelectedAnswerId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserQuizAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserQuizAnswers_QuizAnswers_SelectedAnswerId",
                        column: x => x.SelectedAnswerId,
                        principalTable: "QuizAnswers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserQuizAnswers_QuizQuestions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "QuizQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserQuizAnswers_UserQuizAttempts_AttemptId",
                        column: x => x.AttemptId,
                        principalTable: "UserQuizAttempts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "UpdatedAt",
                value: new DateTime(2025, 12, 11, 16, 48, 3, 217, DateTimeKind.Utc).AddTicks(9295));

            migrationBuilder.CreateIndex(
                name: "IX_UserFlashcardProgresses_FlashcardId",
                table: "UserFlashcardProgresses",
                column: "FlashcardId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFlashcardProgresses_NextReviewDate",
                table: "UserFlashcardProgresses",
                column: "NextReviewDate");

            migrationBuilder.CreateIndex(
                name: "IX_UserFlashcardProgresses_UserId",
                table: "UserFlashcardProgresses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFlashcardProgresses_UserId_FlashcardId",
                table: "UserFlashcardProgresses",
                columns: new[] { "UserId", "FlashcardId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserFlashcardSetAccesses_FlashcardSetId",
                table: "UserFlashcardSetAccesses",
                column: "FlashcardSetId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFlashcardSetAccesses_UserId",
                table: "UserFlashcardSetAccesses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFlashcardSetAccesses_UserId_FlashcardSetId",
                table: "UserFlashcardSetAccesses",
                columns: new[] { "UserId", "FlashcardSetId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserQuizAnswers_AttemptId",
                table: "UserQuizAnswers",
                column: "AttemptId");

            migrationBuilder.CreateIndex(
                name: "IX_UserQuizAnswers_QuestionId",
                table: "UserQuizAnswers",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserQuizAnswers_SelectedAnswerId",
                table: "UserQuizAnswers",
                column: "SelectedAnswerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Удаляем индексы перед удалением таблиц
            migrationBuilder.DropIndex(
                name: "IX_UserFlashcardSetAccesses_LastAccessedAt",
                table: "UserFlashcardSetAccesses");

            migrationBuilder.DropIndex(
                name: "IX_UserFlashcardSetAccesses_IsCompleted",
                table: "UserFlashcardSetAccesses");

            migrationBuilder.DropIndex(
                name: "IX_UserFlashcardProgresses_IsMastered",
                table: "UserFlashcardProgresses");

            migrationBuilder.DropIndex(
                name: "IX_UserFlashcardProgresses_LastReviewedAt",
                table: "UserFlashcardProgresses");

            migrationBuilder.DropTable(
                name: "UserFlashcardProgresses");

            migrationBuilder.DropTable(
                name: "UserFlashcardSetAccesses");

            migrationBuilder.DropTable(
                name: "UserQuizAnswers");

            migrationBuilder.AlterColumn<string>(
                name: "UserAnswersJson",
                table: "UserQuizAttempts",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "UpdatedAt",
                value: new DateTime(2025, 12, 11, 12, 32, 58, 684, DateTimeKind.Utc).AddTicks(3316));

            migrationBuilder.CreateIndex(
                name: "IX_Flashcards_NextReviewDate",
                table: "Flashcards",
                column: "NextReviewDate");
        }
    }
}
