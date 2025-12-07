using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniStart.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestionTypeToExamsRemoveFromQuizzes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QuestionType",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Exams");

            migrationBuilder.AddColumn<string>(
                name: "QuestionType",
                table: "ExamQuestions",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QuestionType",
                table: "ExamQuestions");

            migrationBuilder.AddColumn<string>(
                name: "QuestionType",
                table: "Questions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "Exams",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "Exams",
                type: "timestamp with time zone",
                nullable: true);
        }
    }
}
