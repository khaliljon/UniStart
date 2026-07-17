using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniStart.Migrations
{
    /// <inheritdoc />
    public partial class AddMultipleChoiceSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsMultipleChoice",
                table: "Questions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "MockExamAnswerOptions",
                columns: table => new
                {
                    MockExamAnswerId = table.Column<int>(type: "integer", nullable: false),
                    AnswerOptionId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MockExamAnswerOptions", x => new { x.MockExamAnswerId, x.AnswerOptionId });
                    table.ForeignKey(
                        name: "FK_MockExamAnswerOptions_AnswerOptions_AnswerOptionId",
                        column: x => x.AnswerOptionId,
                        principalTable: "AnswerOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MockExamAnswerOptions_MockExamAnswers_MockExamAnswerId",
                        column: x => x.MockExamAnswerId,
                        principalTable: "MockExamAnswers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MockExamAnswerOptions_AnswerOptionId",
                table: "MockExamAnswerOptions",
                column: "AnswerOptionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MockExamAnswerOptions");

            migrationBuilder.DropColumn(
                name: "IsMultipleChoice",
                table: "Questions");
        }
    }
}
