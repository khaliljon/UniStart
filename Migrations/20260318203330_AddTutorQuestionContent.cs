using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniStart.Migrations
{
    /// <inheritdoc />
    public partial class AddTutorQuestionContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatedByTutorId",
                table: "Questions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPrivate",
                table: "Questions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Questions_CreatedByTutorId",
                table: "Questions",
                column: "CreatedByTutorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Users_CreatedByTutorId",
                table: "Questions",
                column: "CreatedByTutorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Users_CreatedByTutorId",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_Questions_CreatedByTutorId",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "CreatedByTutorId",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "IsPrivate",
                table: "Questions");
        }
    }
}
