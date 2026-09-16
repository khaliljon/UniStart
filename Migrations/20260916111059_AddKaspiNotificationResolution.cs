using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniStart.Migrations
{
    /// <inheritdoc />
    public partial class AddKaspiNotificationResolution : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ResolutionNote",
                table: "KaspiPaymentNotifications",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResolutionType",
                table: "KaspiPaymentNotifications",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResolvedAt",
                table: "KaspiPaymentNotifications",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ResolvedByUserId",
                table: "KaspiPaymentNotifications",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResolutionNote",
                table: "KaspiPaymentNotifications");

            migrationBuilder.DropColumn(
                name: "ResolutionType",
                table: "KaspiPaymentNotifications");

            migrationBuilder.DropColumn(
                name: "ResolvedAt",
                table: "KaspiPaymentNotifications");

            migrationBuilder.DropColumn(
                name: "ResolvedByUserId",
                table: "KaspiPaymentNotifications");
        }
    }
}
