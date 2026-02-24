using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UniStart.Migrations
{
    /// <inheritdoc />
    public partial class AddTestSessionAndTimeSpent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TestSessionId",
                table: "UserAnswers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TimeSpentSeconds",
                table: "UserAnswers",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TestSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    ExamTypeCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Mode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TotalQuestions = table.Column<int>(type: "integer", nullable: false),
                    CorrectCount = table.Column<int>(type: "integer", nullable: false),
                    Score = table.Column<double>(type: "double precision", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestSessions_ExamTypes_ExamTypeCode",
                        column: x => x.ExamTypeCode,
                        principalTable: "ExamTypes",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TestSessions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswers_TestSessionId",
                table: "UserAnswers",
                column: "TestSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_TestSessions_ExamTypeCode",
                table: "TestSessions",
                column: "ExamTypeCode");

            migrationBuilder.CreateIndex(
                name: "IX_TestSessions_UserId",
                table: "TestSessions",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserAnswers_TestSessions_TestSessionId",
                table: "UserAnswers",
                column: "TestSessionId",
                principalTable: "TestSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserAnswers_TestSessions_TestSessionId",
                table: "UserAnswers");

            migrationBuilder.DropTable(
                name: "TestSessions");

            migrationBuilder.DropIndex(
                name: "IX_UserAnswers_TestSessionId",
                table: "UserAnswers");

            migrationBuilder.DropColumn(
                name: "TestSessionId",
                table: "UserAnswers");

            migrationBuilder.DropColumn(
                name: "TimeSpentSeconds",
                table: "UserAnswers");
        }
    }
}
