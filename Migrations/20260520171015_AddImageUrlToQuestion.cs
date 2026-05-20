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
            migrationBuilder.DeleteData(
                table: "ExamTypes",
                keyColumn: "Code",
                keyValue: "CSCA");

            migrationBuilder.DeleteData(
                table: "ExamTypes",
                keyColumn: "Code",
                keyValue: "IELTS");

            migrationBuilder.DeleteData(
                table: "ExamTypes",
                keyColumn: "Code",
                keyValue: "TOEFL");

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

            migrationBuilder.InsertData(
                table: "ExamTypes",
                columns: new[] { "Code", "Name" },
                values: new object[,]
                {
                    { "CSCA", "Gaokao (China College Admission)" },
                    { "IELTS", "IELTS Academic" },
                    { "TOEFL", "TOEFL (Test of English as a Foreign Language)" }
                });
        }
    }
}
