using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UniStart.Migrations
{
    /// <inheritdoc />
    public partial class AddNewsSlugHistoryAndRequiredSlug : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_NewsArticles_Slug",
                table: "NewsArticles");

            // Backfill before NOT NULL + UNIQUE: Id is unique, so these placeholders can never
            // collide. DatabaseSeeder rewrites them into readable slugs on the next startup.
            migrationBuilder.Sql("""
                UPDATE "NewsArticles"
                SET "Slug" = 'news-' || "Id"::text
                WHERE "Slug" IS NULL OR btrim("Slug") = '';
                """);

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "NewsArticles",
                type: "character varying(220)",
                maxLength: 220,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(220)",
                oldMaxLength: 220,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "NewsSlugHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Slug = table.Column<string>(type: "character varying(220)", maxLength: 220, nullable: false),
                    NewsArticleId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsSlugHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NewsSlugHistories_NewsArticles_NewsArticleId",
                        column: x => x.NewsArticleId,
                        principalTable: "NewsArticles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NewsArticles_Slug",
                table: "NewsArticles",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NewsSlugHistories_NewsArticleId",
                table: "NewsSlugHistories",
                column: "NewsArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_NewsSlugHistories_Slug",
                table: "NewsSlugHistories",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NewsSlugHistories");

            migrationBuilder.DropIndex(
                name: "IX_NewsArticles_Slug",
                table: "NewsArticles");

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "NewsArticles",
                type: "character varying(220)",
                maxLength: 220,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(220)",
                oldMaxLength: 220);

            migrationBuilder.CreateIndex(
                name: "IX_NewsArticles_Slug",
                table: "NewsArticles",
                column: "Slug",
                unique: true,
                filter: "\"Slug\" IS NOT NULL");
        }
    }
}
