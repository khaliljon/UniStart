using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace UniStart.Migrations
{
    /// <inheritdoc />
    public partial class AddLearningV2Entities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FlashcardDecks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ExamTypeCode = table.Column<string>(type: "character varying(10)", nullable: true),
                    TopicId = table.Column<int>(type: "integer", nullable: true),
                    IsSystem = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedByUserId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlashcardDecks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FlashcardDecks_ExamTypes_ExamTypeCode",
                        column: x => x.ExamTypeCode,
                        principalTable: "ExamTypes",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_FlashcardDecks_Topics_TopicId",
                        column: x => x.TopicId,
                        principalTable: "Topics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_FlashcardDecks_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "FormulaCards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TopicId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Formula = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormulaCards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormulaCards_Topics_TopicId",
                        column: x => x.TopicId,
                        principalTable: "Topics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LessonSteps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LessonId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    StepType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    QuizQuestionId = table.Column<int>(type: "integer", nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LessonSteps_Questions_QuizQuestionId",
                        column: x => x.QuizQuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_LessonSteps_TopicLessons_LessonId",
                        column: x => x.LessonId,
                        principalTable: "TopicLessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StrategyGuides",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExamTypeCode = table.Column<string>(type: "character varying(10)", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Summary = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    EstimatedReadMinutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 5),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StrategyGuides", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StrategyGuides_ExamTypes_ExamTypeCode",
                        column: x => x.ExamTypeCode,
                        principalTable: "ExamTypes",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TimedDrillResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    DrillType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ExamTypeCode = table.Column<string>(type: "character varying(10)", nullable: true),
                    TopicId = table.Column<int>(type: "integer", nullable: true),
                    QuestionsAnswered = table.Column<int>(type: "integer", nullable: false),
                    CorrectAnswers = table.Column<int>(type: "integer", nullable: false),
                    TotalTimeSeconds = table.Column<int>(type: "integer", nullable: false),
                    AverageTimeSeconds = table.Column<double>(type: "double precision", nullable: false),
                    BestStreak = table.Column<int>(type: "integer", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimedDrillResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimedDrillResults_ExamTypes_ExamTypeCode",
                        column: x => x.ExamTypeCode,
                        principalTable: "ExamTypes",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TimedDrillResults_Topics_TopicId",
                        column: x => x.TopicId,
                        principalTable: "Topics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TimedDrillResults_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserMistakeNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    UserAnswerId = table.Column<int>(type: "integer", nullable: false),
                    ErrorType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    NoteText = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMistakeNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserMistakeNotes_UserAnswers_UserAnswerId",
                        column: x => x.UserAnswerId,
                        principalTable: "UserAnswers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserMistakeNotes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Flashcards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DeckId = table.Column<int>(type: "integer", nullable: false),
                    Front = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Back = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Flashcards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Flashcards_FlashcardDecks_DeckId",
                        column: x => x.DeckId,
                        principalTable: "FlashcardDecks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserFormulaBookmarks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    FormulaCardId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFormulaBookmarks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserFormulaBookmarks_FormulaCards_FormulaCardId",
                        column: x => x.FormulaCardId,
                        principalTable: "FormulaCards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserFormulaBookmarks_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserLessonProgress",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    LessonStepId = table.Column<int>(type: "integer", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLessonProgress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserLessonProgress_LessonSteps_LessonStepId",
                        column: x => x.LessonStepId,
                        principalTable: "LessonSteps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserLessonProgress_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserGuideProgress",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    GuideId = table.Column<int>(type: "integer", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGuideProgress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserGuideProgress_StrategyGuides_GuideId",
                        column: x => x.GuideId,
                        principalTable: "StrategyGuides",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserGuideProgress_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserFlashcardProgress",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    FlashcardId = table.Column<int>(type: "integer", nullable: false),
                    EaseFactor = table.Column<double>(type: "double precision", nullable: false, defaultValue: 2.5),
                    IntervalDays = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    Repetitions = table.Column<int>(type: "integer", nullable: false),
                    NextReviewAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastQuality = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFlashcardProgress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserFlashcardProgress_Flashcards_FlashcardId",
                        column: x => x.FlashcardId,
                        principalTable: "Flashcards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserFlashcardProgress_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ExamTypes",
                columns: new[] { "Code", "Name" },
                values: new object[,]
                {
                    { "CSCA", "Gaokao (China College Admission)" },
                    { "IELTS", "IELTS Academic" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_FlashcardDecks_CreatedByUserId",
                table: "FlashcardDecks",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FlashcardDecks_ExamTypeCode",
                table: "FlashcardDecks",
                column: "ExamTypeCode");

            migrationBuilder.CreateIndex(
                name: "IX_FlashcardDecks_TopicId",
                table: "FlashcardDecks",
                column: "TopicId");

            migrationBuilder.CreateIndex(
                name: "IX_Flashcards_DeckId",
                table: "Flashcards",
                column: "DeckId");

            migrationBuilder.CreateIndex(
                name: "IX_FormulaCards_TopicId",
                table: "FormulaCards",
                column: "TopicId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonSteps_LessonId",
                table: "LessonSteps",
                column: "LessonId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonSteps_QuizQuestionId",
                table: "LessonSteps",
                column: "QuizQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_StrategyGuides_ExamTypeCode",
                table: "StrategyGuides",
                column: "ExamTypeCode");

            migrationBuilder.CreateIndex(
                name: "IX_TimedDrillResults_ExamTypeCode",
                table: "TimedDrillResults",
                column: "ExamTypeCode");

            migrationBuilder.CreateIndex(
                name: "IX_TimedDrillResults_TopicId",
                table: "TimedDrillResults",
                column: "TopicId");

            migrationBuilder.CreateIndex(
                name: "IX_TimedDrillResults_User_Type",
                table: "TimedDrillResults",
                columns: new[] { "UserId", "DrillType" });

            migrationBuilder.CreateIndex(
                name: "IX_UserFlashcardProgress_FlashcardId",
                table: "UserFlashcardProgress",
                column: "FlashcardId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFlashcardProgress_User_Card",
                table: "UserFlashcardProgress",
                columns: new[] { "UserId", "FlashcardId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserFlashcardProgress_User_NextReview",
                table: "UserFlashcardProgress",
                columns: new[] { "UserId", "NextReviewAt" });

            migrationBuilder.CreateIndex(
                name: "IX_UserFormulaBookmarks_FormulaCardId",
                table: "UserFormulaBookmarks",
                column: "FormulaCardId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFormulaBookmarks_User_Formula",
                table: "UserFormulaBookmarks",
                columns: new[] { "UserId", "FormulaCardId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserGuideProgress_GuideId",
                table: "UserGuideProgress",
                column: "GuideId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGuideProgress_User_Guide",
                table: "UserGuideProgress",
                columns: new[] { "UserId", "GuideId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserLessonProgress_LessonStepId",
                table: "UserLessonProgress",
                column: "LessonStepId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLessonProgress_User_Step",
                table: "UserLessonProgress",
                columns: new[] { "UserId", "LessonStepId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserMistakeNotes_User_Answer",
                table: "UserMistakeNotes",
                columns: new[] { "UserId", "UserAnswerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserMistakeNotes_UserAnswerId",
                table: "UserMistakeNotes",
                column: "UserAnswerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TimedDrillResults");

            migrationBuilder.DropTable(
                name: "UserFlashcardProgress");

            migrationBuilder.DropTable(
                name: "UserFormulaBookmarks");

            migrationBuilder.DropTable(
                name: "UserGuideProgress");

            migrationBuilder.DropTable(
                name: "UserLessonProgress");

            migrationBuilder.DropTable(
                name: "UserMistakeNotes");

            migrationBuilder.DropTable(
                name: "Flashcards");

            migrationBuilder.DropTable(
                name: "FormulaCards");

            migrationBuilder.DropTable(
                name: "StrategyGuides");

            migrationBuilder.DropTable(
                name: "LessonSteps");

            migrationBuilder.DropTable(
                name: "FlashcardDecks");

            migrationBuilder.DeleteData(
                table: "ExamTypes",
                keyColumn: "Code",
                keyValue: "CSCA");

            migrationBuilder.DeleteData(
                table: "ExamTypes",
                keyColumn: "Code",
                keyValue: "IELTS");
        }
    }
}
