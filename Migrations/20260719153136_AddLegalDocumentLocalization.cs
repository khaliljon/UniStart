using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniStart.Migrations
{
    /// <inheritdoc />
    public partial class AddLegalDocumentLocalization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContentEn",
                table: "LegalDocuments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContentKz",
                table: "LegalDocuments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastUpdatedLabelEn",
                table: "LegalDocuments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastUpdatedLabelKz",
                table: "LegalDocuments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TitleEn",
                table: "LegalDocuments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TitleKz",
                table: "LegalDocuments",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentEn",
                table: "LegalDocuments");

            migrationBuilder.DropColumn(
                name: "ContentKz",
                table: "LegalDocuments");

            migrationBuilder.DropColumn(
                name: "LastUpdatedLabelEn",
                table: "LegalDocuments");

            migrationBuilder.DropColumn(
                name: "LastUpdatedLabelKz",
                table: "LegalDocuments");

            migrationBuilder.DropColumn(
                name: "TitleEn",
                table: "LegalDocuments");

            migrationBuilder.DropColumn(
                name: "TitleKz",
                table: "LegalDocuments");
        }
    }
}
