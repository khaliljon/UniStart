using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniStart.Migrations
{
    /// <inheritdoc />
    public partial class AddUniversityExamTypesRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExamTypeUniversity",
                columns: table => new
                {
                    ExamTypesId = table.Column<int>(type: "integer", nullable: false),
                    UniversitiesId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamTypeUniversity", x => new { x.ExamTypesId, x.UniversitiesId });
                    table.ForeignKey(
                        name: "FK_ExamTypeUniversity_ExamTypes_ExamTypesId",
                        column: x => x.ExamTypesId,
                        principalTable: "ExamTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamTypeUniversity_Universities_UniversitiesId",
                        column: x => x.UniversitiesId,
                        principalTable: "Universities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExamTypeUniversity_UniversitiesId",
                table: "ExamTypeUniversity",
                column: "UniversitiesId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExamTypeUniversity");
        }
    }
}
