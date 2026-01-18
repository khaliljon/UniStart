using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniStart.Migrations
{
    /// <inheritdoc />
    public partial class RemoveObsoleteFlashcardSRSFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EaseFactor",
                table: "Flashcards");

            migrationBuilder.DropColumn(
                name: "Interval",
                table: "Flashcards");

            migrationBuilder.DropColumn(
                name: "LastReviewedAt",
                table: "Flashcards");

            migrationBuilder.DropColumn(
                name: "NextReviewDate",
                table: "Flashcards");

            migrationBuilder.DropColumn(
                name: "Repetitions",
                table: "Flashcards");

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 18, 14, 19, 44, 484, DateTimeKind.Utc).AddTicks(1005));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "EaseFactor",
                table: "Flashcards",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "Interval",
                table: "Flashcards",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastReviewedAt",
                table: "Flashcards",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextReviewDate",
                table: "Flashcards",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Repetitions",
                table: "Flashcards",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 18, 13, 49, 32, 67, DateTimeKind.Utc).AddTicks(9644));
        }
    }
}
