using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniStart.Migrations
{
    /// <inheritdoc />
    public partial class AddMockAttemptAccessType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccessType",
                table: "MockExamAttempts",
                type: "text",
                nullable: true);

            // Backfill only what IsFree can prove: free stays free. Non-free is
            // left NULL because purchased vs full_access cannot be reconstructed.
            migrationBuilder.Sql(
                "UPDATE \"MockExamAttempts\" SET \"AccessType\" = 'free' WHERE \"IsFree\" = true AND \"AccessType\" IS NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccessType",
                table: "MockExamAttempts");
        }
    }
}
