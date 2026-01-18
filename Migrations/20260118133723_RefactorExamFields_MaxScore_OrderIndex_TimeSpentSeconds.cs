using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniStart.Migrations
{
    /// <inheritdoc />
    public partial class RefactorExamFields_MaxScore_OrderIndex_TimeSpentSeconds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeSpent",
                table: "UserExamAttempts");

            migrationBuilder.RenameColumn(
                name: "TotalPoints",
                table: "UserExamAttempts",
                newName: "TimeSpentSeconds");

            migrationBuilder.RenameColumn(
                name: "TotalPoints",
                table: "Exams",
                newName: "MaxScore");

            migrationBuilder.RenameColumn(
                name: "Order",
                table: "ExamQuestions",
                newName: "OrderIndex");

            migrationBuilder.RenameColumn(
                name: "Order",
                table: "ExamAnswers",
                newName: "OrderIndex");

            migrationBuilder.AddColumn<int>(
                name: "MaxScore",
                table: "UserExamAttempts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 18, 13, 37, 21, 993, DateTimeKind.Utc).AddTicks(7704));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxScore",
                table: "UserExamAttempts");

            migrationBuilder.RenameColumn(
                name: "TimeSpentSeconds",
                table: "UserExamAttempts",
                newName: "TotalPoints");

            migrationBuilder.RenameColumn(
                name: "MaxScore",
                table: "Exams",
                newName: "TotalPoints");

            migrationBuilder.RenameColumn(
                name: "OrderIndex",
                table: "ExamQuestions",
                newName: "Order");

            migrationBuilder.RenameColumn(
                name: "OrderIndex",
                table: "ExamAnswers",
                newName: "Order");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "TimeSpent",
                table: "UserExamAttempts",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 16, 19, 30, 45, 479, DateTimeKind.Utc).AddTicks(3153));
        }
    }
}
