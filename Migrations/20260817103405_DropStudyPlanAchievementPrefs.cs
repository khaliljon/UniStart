using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniStart.Migrations
{
    /// <inheritdoc />
    public partial class DropStudyPlanAchievementPrefs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AchievementNotification",
                table: "NotificationPreferences");

            migrationBuilder.DropColumn(
                name: "StudyPlanReminder",
                table: "NotificationPreferences");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AchievementNotification",
                table: "NotificationPreferences",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "StudyPlanReminder",
                table: "NotificationPreferences",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }
    }
}
