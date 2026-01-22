using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniStart.Migrations
{
    /// <inheritdoc />
    public partial class AddExtendedUserPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EnglishLevel",
                table: "UserPreferences",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "InterestedInScholarships",
                table: "UserPreferences",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxBudgetPerYear",
                table: "UserPreferences",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreferredCity",
                table: "UserPreferences",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreferredCountry",
                table: "UserPreferences",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreferredLanguagesJson",
                table: "UserPreferences",
                type: "jsonb",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "WillingToRelocate",
                table: "UserPreferences",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 21, 16, 0, 7, 861, DateTimeKind.Utc).AddTicks(4531));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnglishLevel",
                table: "UserPreferences");

            migrationBuilder.DropColumn(
                name: "InterestedInScholarships",
                table: "UserPreferences");

            migrationBuilder.DropColumn(
                name: "MaxBudgetPerYear",
                table: "UserPreferences");

            migrationBuilder.DropColumn(
                name: "PreferredCity",
                table: "UserPreferences");

            migrationBuilder.DropColumn(
                name: "PreferredCountry",
                table: "UserPreferences");

            migrationBuilder.DropColumn(
                name: "PreferredLanguagesJson",
                table: "UserPreferences");

            migrationBuilder.DropColumn(
                name: "WillingToRelocate",
                table: "UserPreferences");

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 18, 15, 59, 11, 842, DateTimeKind.Utc).AddTicks(7781));
        }
    }
}
