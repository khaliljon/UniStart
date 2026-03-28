using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniStart.Migrations
{
    /// <inheritdoc />
    public partial class TutorSchoolSubscriptionAndInviteCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RequireApproval",
                table: "TutorSchools",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "SchoolInviteCode",
                table: "TutorSchools",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubscriptionExpiresAt",
                table: "TutorSchools",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubscriptionPaidAt",
                table: "TutorSchools",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubscriptionExpiresAt",
                table: "TutorProfiles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubscriptionPaidAt",
                table: "TutorProfiles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TutorSchools_InviteCode",
                table: "TutorSchools",
                column: "SchoolInviteCode",
                unique: true,
                filter: "\"SchoolInviteCode\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TutorSchools_InviteCode",
                table: "TutorSchools");

            migrationBuilder.DropColumn(
                name: "RequireApproval",
                table: "TutorSchools");

            migrationBuilder.DropColumn(
                name: "SchoolInviteCode",
                table: "TutorSchools");

            migrationBuilder.DropColumn(
                name: "SubscriptionExpiresAt",
                table: "TutorSchools");

            migrationBuilder.DropColumn(
                name: "SubscriptionPaidAt",
                table: "TutorSchools");

            migrationBuilder.DropColumn(
                name: "SubscriptionExpiresAt",
                table: "TutorProfiles");

            migrationBuilder.DropColumn(
                name: "SubscriptionPaidAt",
                table: "TutorProfiles");
        }
    }
}
