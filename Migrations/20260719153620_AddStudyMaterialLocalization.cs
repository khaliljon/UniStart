using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniStart.Migrations
{
    /// <inheritdoc />
    public partial class AddStudyMaterialLocalization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DescriptionEn",
                table: "StudyMaterials",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionKz",
                table: "StudyMaterials",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TitleEn",
                table: "StudyMaterials",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TitleKz",
                table: "StudyMaterials",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DescriptionEn",
                table: "StudyMaterials");

            migrationBuilder.DropColumn(
                name: "DescriptionKz",
                table: "StudyMaterials");

            migrationBuilder.DropColumn(
                name: "TitleEn",
                table: "StudyMaterials");

            migrationBuilder.DropColumn(
                name: "TitleKz",
                table: "StudyMaterials");
        }
    }
}
