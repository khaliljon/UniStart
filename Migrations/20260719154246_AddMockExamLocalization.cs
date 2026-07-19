using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniStart.Migrations
{
    /// <inheritdoc />
    public partial class AddMockExamLocalization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DescriptionEn",
                table: "MockExams",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionKz",
                table: "MockExams",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TitleEn",
                table: "MockExams",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TitleKz",
                table: "MockExams",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DescriptionEn",
                table: "MockExams");

            migrationBuilder.DropColumn(
                name: "DescriptionKz",
                table: "MockExams");

            migrationBuilder.DropColumn(
                name: "TitleEn",
                table: "MockExams");

            migrationBuilder.DropColumn(
                name: "TitleKz",
                table: "MockExams");
        }
    }
}
