using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniStart.Migrations
{
    /// <inheritdoc />
    public partial class AddIsSyntheticDataToUserFlashcardProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSyntheticData",
                table: "UserFlashcardProgresses",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "SiteDescription", "UpdatedAt" },
                values: new object[] { "Образовательная платформа для изучения с помощью карточек и квизов", new DateTime(2026, 1, 15, 14, 30, 10, 456, DateTimeKind.Utc).AddTicks(9597) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSyntheticData",
                table: "UserFlashcardProgresses");

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "SiteDescription", "UpdatedAt" },
                values: new object[] { "Образовательная платформа для изучения с помощью карточек и тестов", new DateTime(2026, 1, 8, 15, 14, 17, 12, DateTimeKind.Utc).AddTicks(1497) });
        }
    }
}
