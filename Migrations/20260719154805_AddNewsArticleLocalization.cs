using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniStart.Migrations
{
    /// <inheritdoc />
    public partial class AddNewsArticleLocalization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BodyEn",
                table: "NewsArticles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BodyKz",
                table: "NewsArticles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SummaryEn",
                table: "NewsArticles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SummaryKz",
                table: "NewsArticles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TitleEn",
                table: "NewsArticles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TitleKz",
                table: "NewsArticles",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BodyEn",
                table: "NewsArticles");

            migrationBuilder.DropColumn(
                name: "BodyKz",
                table: "NewsArticles");

            migrationBuilder.DropColumn(
                name: "SummaryEn",
                table: "NewsArticles");

            migrationBuilder.DropColumn(
                name: "SummaryKz",
                table: "NewsArticles");

            migrationBuilder.DropColumn(
                name: "TitleEn",
                table: "NewsArticles");

            migrationBuilder.DropColumn(
                name: "TitleKz",
                table: "NewsArticles");
        }
    }
}
