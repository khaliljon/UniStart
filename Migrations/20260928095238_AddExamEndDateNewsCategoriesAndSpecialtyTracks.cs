using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UniStart.Migrations
{
    /// <inheritdoc />
    public partial class AddExamEndDateNewsCategoriesAndSpecialtyTracks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "NewsArticles",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "admission");

            migrationBuilder.AddColumn<bool>(
                name: "IsFeatured",
                table: "NewsArticles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "NewsArticles",
                type: "character varying(220)",
                maxLength: 220,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "EndDate",
                table: "ExamSittings",
                type: "date",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SpecialtyTracks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    NameKz = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    NameEn = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    Subjects = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ConditionalChinese = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpecialtyTracks", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "ExamSittings",
                keyColumn: "Id",
                keyValue: 1,
                column: "EndDate",
                value: null);

            migrationBuilder.UpdateData(
                table: "ExamSittings",
                keyColumn: "Id",
                keyValue: 2,
                column: "EndDate",
                value: null);

            migrationBuilder.UpdateData(
                table: "ExamSittings",
                keyColumn: "Id",
                keyValue: 3,
                column: "EndDate",
                value: null);

            migrationBuilder.UpdateData(
                table: "ExamSittings",
                keyColumn: "Id",
                keyValue: 4,
                column: "EndDate",
                value: null);

            migrationBuilder.UpdateData(
                table: "ExamSittings",
                keyColumn: "Id",
                keyValue: 5,
                column: "EndDate",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_NewsArticles_Slug",
                table: "NewsArticles",
                column: "Slug",
                unique: true,
                filter: "\"Slug\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SpecialtyTracks_SortOrder",
                table: "SpecialtyTracks",
                column: "SortOrder");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SpecialtyTracks");

            migrationBuilder.DropIndex(
                name: "IX_NewsArticles_Slug",
                table: "NewsArticles");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "NewsArticles");

            migrationBuilder.DropColumn(
                name: "IsFeatured",
                table: "NewsArticles");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "NewsArticles");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "ExamSittings");
        }
    }
}
