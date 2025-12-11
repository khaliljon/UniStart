using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UniStart.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SystemSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SiteName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SiteDescription = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    AllowRegistration = table.Column<bool>(type: "boolean", nullable: false),
                    RequireEmailVerification = table.Column<bool>(type: "boolean", nullable: false),
                    MaxQuizAttempts = table.Column<int>(type: "integer", nullable: false),
                    SessionTimeout = table.Column<int>(type: "integer", nullable: false),
                    EnableNotifications = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSettings", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "SystemSettings",
                columns: new[] { "Id", "AllowRegistration", "EnableNotifications", "MaxQuizAttempts", "RequireEmailVerification", "SessionTimeout", "SiteDescription", "SiteName", "UpdatedAt" },
                values: new object[] { 1, true, true, 3, false, 30, "Образовательная платформа для изучения с помощью карточек и тестов", "UniStart", new DateTime(2025, 12, 11, 12, 32, 58, 684, DateTimeKind.Utc).AddTicks(3316) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SystemSettings");
        }
    }
}
