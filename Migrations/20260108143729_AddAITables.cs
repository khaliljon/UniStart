using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniStart.Migrations
{
    /// <inheritdoc />
    public partial class AddAITables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UniversityRecommendation_AspNetUsers_UserId",
                table: "UniversityRecommendation");

            migrationBuilder.DropForeignKey(
                name: "FK_UniversityRecommendation_Universities_UniversityId",
                table: "UniversityRecommendation");

            migrationBuilder.DropForeignKey(
                name: "FK_UserLearningPattern_AspNetUsers_UserId",
                table: "UserLearningPattern");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserLearningPattern",
                table: "UserLearningPattern");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UniversityRecommendation",
                table: "UniversityRecommendation");

            migrationBuilder.RenameTable(
                name: "UserLearningPattern",
                newName: "UserLearningPatterns");

            migrationBuilder.RenameTable(
                name: "UniversityRecommendation",
                newName: "UniversityRecommendations");

            migrationBuilder.RenameIndex(
                name: "IX_UserLearningPattern_UserId",
                table: "UserLearningPatterns",
                newName: "IX_UserLearningPatterns_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UniversityRecommendation_UserId",
                table: "UniversityRecommendations",
                newName: "IX_UniversityRecommendations_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UniversityRecommendation_UniversityId",
                table: "UniversityRecommendations",
                newName: "IX_UniversityRecommendations_UniversityId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserLearningPatterns",
                table: "UserLearningPatterns",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UniversityRecommendations",
                table: "UniversityRecommendations",
                column: "Id");

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 8, 14, 37, 28, 455, DateTimeKind.Utc).AddTicks(4236));

            migrationBuilder.AddForeignKey(
                name: "FK_UniversityRecommendations_AspNetUsers_UserId",
                table: "UniversityRecommendations",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UniversityRecommendations_Universities_UniversityId",
                table: "UniversityRecommendations",
                column: "UniversityId",
                principalTable: "Universities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserLearningPatterns_AspNetUsers_UserId",
                table: "UserLearningPatterns",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UniversityRecommendations_AspNetUsers_UserId",
                table: "UniversityRecommendations");

            migrationBuilder.DropForeignKey(
                name: "FK_UniversityRecommendations_Universities_UniversityId",
                table: "UniversityRecommendations");

            migrationBuilder.DropForeignKey(
                name: "FK_UserLearningPatterns_AspNetUsers_UserId",
                table: "UserLearningPatterns");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserLearningPatterns",
                table: "UserLearningPatterns");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UniversityRecommendations",
                table: "UniversityRecommendations");

            migrationBuilder.RenameTable(
                name: "UserLearningPatterns",
                newName: "UserLearningPattern");

            migrationBuilder.RenameTable(
                name: "UniversityRecommendations",
                newName: "UniversityRecommendation");

            migrationBuilder.RenameIndex(
                name: "IX_UserLearningPatterns_UserId",
                table: "UserLearningPattern",
                newName: "IX_UserLearningPattern_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UniversityRecommendations_UserId",
                table: "UniversityRecommendation",
                newName: "IX_UniversityRecommendation_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UniversityRecommendations_UniversityId",
                table: "UniversityRecommendation",
                newName: "IX_UniversityRecommendation_UniversityId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserLearningPattern",
                table: "UserLearningPattern",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UniversityRecommendation",
                table: "UniversityRecommendation",
                column: "Id");

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 8, 13, 51, 0, 371, DateTimeKind.Utc).AddTicks(9832));

            migrationBuilder.AddForeignKey(
                name: "FK_UniversityRecommendation_AspNetUsers_UserId",
                table: "UniversityRecommendation",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UniversityRecommendation_Universities_UniversityId",
                table: "UniversityRecommendation",
                column: "UniversityId",
                principalTable: "Universities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserLearningPattern_AspNetUsers_UserId",
                table: "UserLearningPattern",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
