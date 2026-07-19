using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace UniStart.Migrations
{
    /// <inheritdoc />
    public partial class AddExamSittings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExamSittings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamSittings", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "ExamSittings",
                columns: new[] { "Id", "Date", "IsActive", "SortOrder" },
                values: new object[,]
                {
                    { 1, new DateOnly(2026, 1, 17), true, 1 },
                    { 2, new DateOnly(2026, 3, 15), true, 2 },
                    { 3, new DateOnly(2026, 6, 27), true, 3 },
                    { 4, new DateOnly(2026, 9, 19), true, 4 },
                    { 5, new DateOnly(2026, 11, 21), true, 5 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExamSittings");
        }
    }
}
