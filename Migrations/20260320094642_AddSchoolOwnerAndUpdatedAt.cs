using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniStart.Migrations
{
    /// <inheritdoc />
    public partial class AddSchoolOwnerAndUpdatedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OwnerUserId",
                table: "TutorSchools",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "TutorSchools",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TutorSchools_OwnerUserId",
                table: "TutorSchools",
                column: "OwnerUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TutorSchools_Users_OwnerUserId",
                table: "TutorSchools",
                column: "OwnerUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TutorSchools_Users_OwnerUserId",
                table: "TutorSchools");

            migrationBuilder.DropIndex(
                name: "IX_TutorSchools_OwnerUserId",
                table: "TutorSchools");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "TutorSchools");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "TutorSchools");
        }
    }
}
