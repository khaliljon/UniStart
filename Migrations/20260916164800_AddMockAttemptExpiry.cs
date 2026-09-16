using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniStart.Migrations
{
    /// <inheritdoc />
    public partial class AddMockAttemptExpiry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CompletionReason",
                table: "MockExamAttempts",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "MockExamAttempts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MockExamAttempts_Status_ExpiresAt",
                table: "MockExamAttempts",
                columns: new[] { "Status", "ExpiresAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MockExamAttempts_Status_ExpiresAt",
                table: "MockExamAttempts");

            migrationBuilder.DropColumn(
                name: "CompletionReason",
                table: "MockExamAttempts");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "MockExamAttempts");
        }
    }
}
