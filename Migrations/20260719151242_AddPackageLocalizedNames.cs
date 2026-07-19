using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniStart.Migrations
{
    /// <inheritdoc />
    public partial class AddPackageLocalizedNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NameEn",
                table: "MockPackages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameKz",
                table: "MockPackages",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NameEn",
                table: "MockPackages");

            migrationBuilder.DropColumn(
                name: "NameKz",
                table: "MockPackages");
        }
    }
}
