using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UniStart.Migrations
{
    /// <inheritdoc />
    public partial class AddUserPreferencesWithSubjectCollections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserPreferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LearningGoal = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TargetExamType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TargetUniversityId = table.Column<int>(type: "integer", nullable: true),
                    CareerGoal = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PrefersFlashcards = table.Column<bool>(type: "boolean", nullable: false),
                    PrefersQuizzes = table.Column<bool>(type: "boolean", nullable: false),
                    PrefersExams = table.Column<bool>(type: "boolean", nullable: false),
                    PreferredDifficulty = table.Column<int>(type: "integer", nullable: false),
                    DailyStudyTimeMinutes = table.Column<int>(type: "integer", nullable: false),
                    PreferredStudyTime = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    StudyDaysJson = table.Column<string>(type: "jsonb", nullable: false),
                    MotivationLevel = table.Column<int>(type: "integer", nullable: false),
                    NeedsReminders = table.Column<bool>(type: "boolean", nullable: false),
                    OnboardingCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPreferences_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserPreferences_Universities_TargetUniversityId",
                        column: x => x.TargetUniversityId,
                        principalTable: "Universities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "UserPreferencesInterestedSubject",
                columns: table => new
                {
                    UserPreferencesId = table.Column<int>(type: "integer", nullable: false),
                    SubjectsId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPreferencesInterestedSubject", x => new { x.UserPreferencesId, x.SubjectsId });
                    table.ForeignKey(
                        name: "FK_UserPreferencesInterestedSubject_Subjects_SubjectsId",
                        column: x => x.SubjectsId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserPreferencesInterestedSubject_UserPreferences_UserPrefer~",
                        column: x => x.UserPreferencesId,
                        principalTable: "UserPreferences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserPreferencesStrongSubject",
                columns: table => new
                {
                    UserPreferencesId = table.Column<int>(type: "integer", nullable: false),
                    SubjectsId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPreferencesStrongSubject", x => new { x.UserPreferencesId, x.SubjectsId });
                    table.ForeignKey(
                        name: "FK_UserPreferencesStrongSubject_Subjects_SubjectsId",
                        column: x => x.SubjectsId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserPreferencesStrongSubject_UserPreferences_UserPreference~",
                        column: x => x.UserPreferencesId,
                        principalTable: "UserPreferences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserPreferencesWeakSubject",
                columns: table => new
                {
                    UserPreferencesId = table.Column<int>(type: "integer", nullable: false),
                    SubjectsId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPreferencesWeakSubject", x => new { x.UserPreferencesId, x.SubjectsId });
                    table.ForeignKey(
                        name: "FK_UserPreferencesWeakSubject_Subjects_SubjectsId",
                        column: x => x.SubjectsId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserPreferencesWeakSubject_UserPreferences_UserPreferencesId",
                        column: x => x.UserPreferencesId,
                        principalTable: "UserPreferences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 18, 15, 30, 17, 387, DateTimeKind.Utc).AddTicks(4462));

            migrationBuilder.CreateIndex(
                name: "IX_UserPreferences_TargetUniversityId",
                table: "UserPreferences",
                column: "TargetUniversityId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPreferences_UserId",
                table: "UserPreferences",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserPreferencesInterestedSubject_SubjectsId",
                table: "UserPreferencesInterestedSubject",
                column: "SubjectsId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPreferencesStrongSubject_SubjectsId",
                table: "UserPreferencesStrongSubject",
                column: "SubjectsId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPreferencesWeakSubject_SubjectsId",
                table: "UserPreferencesWeakSubject",
                column: "SubjectsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserPreferencesInterestedSubject");

            migrationBuilder.DropTable(
                name: "UserPreferencesStrongSubject");

            migrationBuilder.DropTable(
                name: "UserPreferencesWeakSubject");

            migrationBuilder.DropTable(
                name: "UserPreferences");

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 18, 15, 21, 59, 665, DateTimeKind.Utc).AddTicks(1114));
        }
    }
}
