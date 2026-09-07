using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniStart.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestionAndMockLanguage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserMockRuns_UserId_MockExamId",
                table: "UserMockRuns");

            migrationBuilder.DropIndex(
                name: "IX_UserCartItems_UserId_ItemType_ItemCode",
                table: "UserCartItems");

            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "UserMockRuns",
                type: "character varying(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "en");

            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "UserCartItems",
                type: "character varying(8)",
                maxLength: 8,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "Questions",
                type: "character varying(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "en");

            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "QuestionImportJobs",
                type: "character varying(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "en");

            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "MockExamAttempts",
                type: "character varying(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "en");

            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "ImportedQuestionDrafts",
                type: "character varying(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "en");

            migrationBuilder.CreateIndex(
                name: "IX_UserMockRuns_UserId_MockExamId_Language",
                table: "UserMockRuns",
                columns: new[] { "UserId", "MockExamId", "Language" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserCartItems_UserId_ItemType_ItemCode_Language",
                table: "UserCartItems",
                columns: new[] { "UserId", "ItemType", "ItemCode", "Language" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserMockRuns_UserId_MockExamId_Language",
                table: "UserMockRuns");

            migrationBuilder.DropIndex(
                name: "IX_UserCartItems_UserId_ItemType_ItemCode_Language",
                table: "UserCartItems");

            migrationBuilder.DropColumn(
                name: "Language",
                table: "UserMockRuns");

            migrationBuilder.DropColumn(
                name: "Language",
                table: "UserCartItems");

            migrationBuilder.DropColumn(
                name: "Language",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "Language",
                table: "QuestionImportJobs");

            migrationBuilder.DropColumn(
                name: "Language",
                table: "MockExamAttempts");

            migrationBuilder.DropColumn(
                name: "Language",
                table: "ImportedQuestionDrafts");

            migrationBuilder.CreateIndex(
                name: "IX_UserMockRuns_UserId_MockExamId",
                table: "UserMockRuns",
                columns: new[] { "UserId", "MockExamId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserCartItems_UserId_ItemType_ItemCode",
                table: "UserCartItems",
                columns: new[] { "UserId", "ItemType", "ItemCode" },
                unique: true);
        }
    }
}
