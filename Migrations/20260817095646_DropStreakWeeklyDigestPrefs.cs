using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniStart.Migrations
{
    /// <inheritdoc />
    public partial class DropStreakWeeklyDigestPrefs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastStreakReminderSentAt",
                table: "NotificationPreferences");

            migrationBuilder.DropColumn(
                name: "LastWeeklyDigestSentAt",
                table: "NotificationPreferences");

            migrationBuilder.DropColumn(
                name: "StreakReminder",
                table: "NotificationPreferences");

            migrationBuilder.DropColumn(
                name: "WeeklyDigest",
                table: "NotificationPreferences");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastStreakReminderSentAt",
                table: "NotificationPreferences",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastWeeklyDigestSentAt",
                table: "NotificationPreferences",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "StreakReminder",
                table: "NotificationPreferences",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "WeeklyDigest",
                table: "NotificationPreferences",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }
    }
}
