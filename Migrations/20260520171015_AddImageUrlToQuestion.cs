using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace UniStart.Migrations
{
    /// <inheritdoc />
    public partial class AddImageUrlToQuestion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // NOTE: the original auto-generated migration also deleted the CSCA/IELTS/TOEFL
            // seed exam types here. That delete crashes on databases that still have rows
            // (e.g. TestSessions) referencing those codes (FK violation). Removed — leaving
            // unused exam types in place is harmless; they can be cleaned up separately
            // after their dependent data is removed.
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Questions",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Questions");
        }
    }
}
