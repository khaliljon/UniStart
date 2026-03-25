using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniStart.Migrations
{
    /// <inheritdoc />
    public partial class AddSchoolBrandingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccentColor",
                table: "TutorSchools",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomDomain",
                table: "TutorSchools",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NavbarTitle",
                table: "TutorSchools",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrimaryColor",
                table: "TutorSchools",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrimaryHoverColor",
                table: "TutorSchools",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Subdomain",
                table: "TutorSchools",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccentColor",
                table: "TutorSchools");

            migrationBuilder.DropColumn(
                name: "CustomDomain",
                table: "TutorSchools");

            migrationBuilder.DropColumn(
                name: "NavbarTitle",
                table: "TutorSchools");

            migrationBuilder.DropColumn(
                name: "PrimaryColor",
                table: "TutorSchools");

            migrationBuilder.DropColumn(
                name: "PrimaryHoverColor",
                table: "TutorSchools");

            migrationBuilder.DropColumn(
                name: "Subdomain",
                table: "TutorSchools");
        }
    }
}
