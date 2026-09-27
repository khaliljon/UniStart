using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniStart.Migrations
{
    /// <inheritdoc />
    public partial class AddExamSittingEndDateAndNewsCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "NewsArticles",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "admission");

            migrationBuilder.AddColumn<bool>(
                name: "IsFeatured",
                table: "NewsArticles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "NewsArticles",
                type: "character varying(220)",
                maxLength: 220,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "EndDate",
                table: "ExamSittings",
                type: "date",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "ExamSittings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Date", "EndDate" },
                values: new object[] { new DateOnly(2026, 11, 14), new DateOnly(2026, 11, 15) });

            migrationBuilder.UpdateData(
                table: "ExamSittings",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Date", "EndDate" },
                values: new object[] { new DateOnly(2026, 12, 19), new DateOnly(2026, 12, 20) });

            migrationBuilder.UpdateData(
                table: "ExamSittings",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Date", "EndDate" },
                values: new object[] { new DateOnly(2027, 1, 23), new DateOnly(2027, 1, 24) });

            migrationBuilder.UpdateData(
                table: "ExamSittings",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Date", "EndDate" },
                values: new object[] { new DateOnly(2027, 3, 13), new DateOnly(2027, 3, 14) });

            migrationBuilder.UpdateData(
                table: "ExamSittings",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Date", "EndDate" },
                values: new object[] { new DateOnly(2027, 4, 24), new DateOnly(2027, 4, 25) });

            migrationBuilder.InsertData(
                table: "ExamSittings",
                columns: new[] { "Id", "Date", "EndDate", "IsActive", "SortOrder" },
                values: new object[] { 6, new DateOnly(2027, 6, 26), new DateOnly(2027, 6, 27), true, 6 });

            migrationBuilder.CreateIndex(
                name: "IX_NewsArticles_Slug",
                table: "NewsArticles",
                column: "Slug",
                unique: true,
                filter: "\"Slug\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_NewsArticles_Slug",
                table: "NewsArticles");

            migrationBuilder.DeleteData(
                table: "ExamSittings",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DropColumn(
                name: "Category",
                table: "NewsArticles");

            migrationBuilder.DropColumn(
                name: "IsFeatured",
                table: "NewsArticles");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "NewsArticles");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "ExamSittings");

            migrationBuilder.UpdateData(
                table: "ExamSittings",
                keyColumn: "Id",
                keyValue: 1,
                column: "Date",
                value: new DateOnly(2026, 1, 17));

            migrationBuilder.UpdateData(
                table: "ExamSittings",
                keyColumn: "Id",
                keyValue: 2,
                column: "Date",
                value: new DateOnly(2026, 3, 15));

            migrationBuilder.UpdateData(
                table: "ExamSittings",
                keyColumn: "Id",
                keyValue: 3,
                column: "Date",
                value: new DateOnly(2026, 6, 27));

            migrationBuilder.UpdateData(
                table: "ExamSittings",
                keyColumn: "Id",
                keyValue: 4,
                column: "Date",
                value: new DateOnly(2026, 9, 19));

            migrationBuilder.UpdateData(
                table: "ExamSittings",
                keyColumn: "Id",
                keyValue: 5,
                column: "Date",
                value: new DateOnly(2026, 11, 21));
        }
    }
}
