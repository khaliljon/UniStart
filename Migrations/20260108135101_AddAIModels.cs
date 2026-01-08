using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UniStart.Migrations
{
    /// <inheritdoc />
    public partial class AddAIModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MinScore",
                table: "Universities",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProgramsJson",
                table: "Universities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StrongSubjectsJson",
                table: "Universities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TuitionFee",
                table: "Universities",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CareerGoal",
                table: "AspNetUsers",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxBudget",
                table: "AspNetUsers",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreferredCity",
                table: "AspNetUsers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UniversityRecommendation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    UniversityId = table.Column<int>(type: "integer", nullable: false),
                    MatchScore = table.Column<double>(type: "double precision", nullable: false),
                    AdmissionProbability = table.Column<double>(type: "double precision", nullable: false),
                    ReasonsJson = table.Column<string>(type: "jsonb", nullable: false),
                    Rank = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsViewed = table.Column<bool>(type: "boolean", nullable: false),
                    ViewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UserRating = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UniversityRecommendation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UniversityRecommendation_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UniversityRecommendation_Universities_UniversityId",
                        column: x => x.UniversityId,
                        principalTable: "Universities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserLearningPattern",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    AverageRetentionRate = table.Column<double>(type: "double precision", nullable: false),
                    SubjectMasteryJson = table.Column<string>(type: "jsonb", nullable: false),
                    PreferredStudyHour = table.Column<int>(type: "integer", nullable: true),
                    AverageSessionMinutes = table.Column<int>(type: "integer", nullable: true),
                    OptimalDifficulty = table.Column<double>(type: "double precision", nullable: false),
                    ForgettingSpeed = table.Column<double>(type: "double precision", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SessionsProcessed = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLearningPattern", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserLearningPattern_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 8, 13, 51, 0, 371, DateTimeKind.Utc).AddTicks(9832));

            migrationBuilder.CreateIndex(
                name: "IX_UniversityRecommendation_UniversityId",
                table: "UniversityRecommendation",
                column: "UniversityId");

            migrationBuilder.CreateIndex(
                name: "IX_UniversityRecommendation_UserId",
                table: "UniversityRecommendation",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLearningPattern_UserId",
                table: "UserLearningPattern",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UniversityRecommendation");

            migrationBuilder.DropTable(
                name: "UserLearningPattern");

            migrationBuilder.DropColumn(
                name: "MinScore",
                table: "Universities");

            migrationBuilder.DropColumn(
                name: "ProgramsJson",
                table: "Universities");

            migrationBuilder.DropColumn(
                name: "StrongSubjectsJson",
                table: "Universities");

            migrationBuilder.DropColumn(
                name: "TuitionFee",
                table: "Universities");

            migrationBuilder.DropColumn(
                name: "CareerGoal",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "MaxBudget",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PreferredCity",
                table: "AspNetUsers");

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 2, 14, 57, 38, 263, DateTimeKind.Utc).AddTicks(257));
        }
    }
}
