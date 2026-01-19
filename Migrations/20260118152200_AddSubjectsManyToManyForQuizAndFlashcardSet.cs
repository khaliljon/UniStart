using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniStart.Migrations
{
    /// <inheritdoc />
    public partial class AddSubjectsManyToManyForQuizAndFlashcardSet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FlashcardSetSubject",
                columns: table => new
                {
                    FlashcardSetsId = table.Column<int>(type: "integer", nullable: false),
                    SubjectsId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlashcardSetSubject", x => new { x.FlashcardSetsId, x.SubjectsId });
                    table.ForeignKey(
                        name: "FK_FlashcardSetSubject_FlashcardSets_FlashcardSetsId",
                        column: x => x.FlashcardSetsId,
                        principalTable: "FlashcardSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FlashcardSetSubject_Subjects_SubjectsId",
                        column: x => x.SubjectsId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuizSubject",
                columns: table => new
                {
                    QuizzesId = table.Column<int>(type: "integer", nullable: false),
                    SubjectsId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizSubject", x => new { x.QuizzesId, x.SubjectsId });
                    table.ForeignKey(
                        name: "FK_QuizSubject_Quizzes_QuizzesId",
                        column: x => x.QuizzesId,
                        principalTable: "Quizzes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuizSubject_Subjects_SubjectsId",
                        column: x => x.SubjectsId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "SiteDescription", "UpdatedAt" },
                values: new object[] { "��������������� ��������� ��� �������� � ������� �������� � ������", new DateTime(2026, 1, 18, 15, 21, 59, 665, DateTimeKind.Utc).AddTicks(1114) });

            migrationBuilder.CreateIndex(
                name: "IX_FlashcardSetSubject_SubjectsId",
                table: "FlashcardSetSubject",
                column: "SubjectsId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizSubject_SubjectsId",
                table: "QuizSubject",
                column: "SubjectsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FlashcardSetSubject");

            migrationBuilder.DropTable(
                name: "QuizSubject");

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "SiteDescription", "UpdatedAt" },
                values: new object[] { "Образовательная платформа для изучения с помощью карточек и квизов", new DateTime(2026, 1, 18, 14, 19, 44, 484, DateTimeKind.Utc).AddTicks(1005) });
        }
    }
}
