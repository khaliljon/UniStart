using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UniStart.Migrations
{
    /// <inheritdoc />
    public partial class AddRunBasedMockCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MockPackages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Key = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    PickCount = table.Column<int>(type: "integer", nullable: false),
                    RunsEach = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    Currency = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false, defaultValue: "KZT"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MockPackages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MockPriceTiers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MockExamId = table.Column<int>(type: "integer", nullable: false),
                    Runs = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    Currency = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false, defaultValue: "KZT"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MockPriceTiers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MockPriceTiers_MockExams_MockExamId",
                        column: x => x.MockExamId,
                        principalTable: "MockExams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserMockRuns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    MockExamId = table.Column<int>(type: "integer", nullable: false),
                    RunsRemaining = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMockRuns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserMockRuns_MockExams_MockExamId",
                        column: x => x.MockExamId,
                        principalTable: "MockExams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserMockRuns_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MockPackages_Key",
                table: "MockPackages",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MockPriceTiers_MockExamId_Runs",
                table: "MockPriceTiers",
                columns: new[] { "MockExamId", "Runs" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserMockRuns_MockExamId",
                table: "UserMockRuns",
                column: "MockExamId");

            migrationBuilder.CreateIndex(
                name: "IX_UserMockRuns_UserId_MockExamId",
                table: "UserMockRuns",
                columns: new[] { "UserId", "MockExamId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MockPackages");

            migrationBuilder.DropTable(
                name: "MockPriceTiers");

            migrationBuilder.DropTable(
                name: "UserMockRuns");
        }
    }
}
