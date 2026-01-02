using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniStart.Migrations
{
    /// <inheritdoc />
    public partial class FixUserAnswerDeleteBehavior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserExamAnswers_ExamAnswers_SelectedAnswerId",
                table: "UserExamAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_UserExamAnswers_ExamQuestions_QuestionId",
                table: "UserExamAnswers");

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "UpdatedAt",
                value: new DateTime(2025, 12, 12, 18, 22, 54, 173, DateTimeKind.Utc).AddTicks(3790));

            migrationBuilder.AddForeignKey(
                name: "FK_UserExamAnswers_ExamAnswers_SelectedAnswerId",
                table: "UserExamAnswers",
                column: "SelectedAnswerId",
                principalTable: "ExamAnswers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserExamAnswers_ExamQuestions_QuestionId",
                table: "UserExamAnswers",
                column: "QuestionId",
                principalTable: "ExamQuestions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserExamAnswers_ExamAnswers_SelectedAnswerId",
                table: "UserExamAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_UserExamAnswers_ExamQuestions_QuestionId",
                table: "UserExamAnswers");

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "UpdatedAt",
                value: new DateTime(2025, 12, 11, 16, 48, 3, 217, DateTimeKind.Utc).AddTicks(9295));

            migrationBuilder.AddForeignKey(
                name: "FK_UserExamAnswers_ExamAnswers_SelectedAnswerId",
                table: "UserExamAnswers",
                column: "SelectedAnswerId",
                principalTable: "ExamAnswers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserExamAnswers_ExamQuestions_QuestionId",
                table: "UserExamAnswers",
                column: "QuestionId",
                principalTable: "ExamQuestions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
