using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniStart.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSubjectFromQuizAndFlashcardSet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Subject",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "Subject",
                table: "FlashcardSets");

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 18, 15, 59, 11, 842, DateTimeKind.Utc).AddTicks(7781));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Subject",
                table: "Quizzes",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Subject",
                table: "FlashcardSets",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 18, 15, 30, 17, 387, DateTimeKind.Utc).AddTicks(4462));
        }
    }
}
