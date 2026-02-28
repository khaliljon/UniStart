using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniStart.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserAnswers_UserId",
                table: "UserAnswers");

            migrationBuilder.DropIndex(
                name: "IX_TestSessions_UserId",
                table: "TestSessions");

            migrationBuilder.DropIndex(
                name: "IX_StudyPlanEntries_PlanId",
                table: "StudyPlanEntries");

            migrationBuilder.DropIndex(
                name: "IX_MockExamAttempts_UserId",
                table: "MockExamAttempts");

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswers_UserId_AnsweredAt",
                table: "UserAnswers",
                columns: new[] { "UserId", "AnsweredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswers_UserId_QuestionId_SessionId",
                table: "UserAnswers",
                columns: new[] { "UserId", "QuestionId", "TestSessionId" });

            migrationBuilder.CreateIndex(
                name: "IX_TestSessions_UserId_StartedAt",
                table: "TestSessions",
                columns: new[] { "UserId", "StartedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_StudyPlanEntries_PlanId_Date",
                table: "StudyPlanEntries",
                columns: new[] { "PlanId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_MockExamAttempts_UserId_Status",
                table: "MockExamAttempts",
                columns: new[] { "UserId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserAnswers_UserId_AnsweredAt",
                table: "UserAnswers");

            migrationBuilder.DropIndex(
                name: "IX_UserAnswers_UserId_QuestionId_SessionId",
                table: "UserAnswers");

            migrationBuilder.DropIndex(
                name: "IX_TestSessions_UserId_StartedAt",
                table: "TestSessions");

            migrationBuilder.DropIndex(
                name: "IX_StudyPlanEntries_PlanId_Date",
                table: "StudyPlanEntries");

            migrationBuilder.DropIndex(
                name: "IX_MockExamAttempts_UserId_Status",
                table: "MockExamAttempts");

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswers_UserId",
                table: "UserAnswers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TestSessions_UserId",
                table: "TestSessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_StudyPlanEntries_PlanId",
                table: "StudyPlanEntries",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_MockExamAttempts_UserId",
                table: "MockExamAttempts",
                column: "UserId");
        }
    }
}
