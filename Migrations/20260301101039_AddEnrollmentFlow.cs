using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniStart.Migrations
{
    /// <inheritdoc />
    public partial class AddEnrollmentFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeclineReason",
                table: "Conversations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeclinedAt",
                table: "Conversations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestMessage",
                table: "Conversations",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeclineReason",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "DeclinedAt",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "RequestMessage",
                table: "Conversations");
        }
    }
}
