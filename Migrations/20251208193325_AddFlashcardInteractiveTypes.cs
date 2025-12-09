using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniStart.Migrations
{
    /// <inheritdoc />
    public partial class AddFlashcardInteractiveTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MatchingPairsJson",
                table: "Flashcards",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OptionsJson",
                table: "Flashcards",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SequenceJson",
                table: "Flashcards",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Flashcards",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MatchingPairsJson",
                table: "Flashcards");

            migrationBuilder.DropColumn(
                name: "OptionsJson",
                table: "Flashcards");

            migrationBuilder.DropColumn(
                name: "SequenceJson",
                table: "Flashcards");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Flashcards");
        }
    }
}
