using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniStart.Migrations
{
    /// <inheritdoc />
    public partial class SchoolTutorRoleAndSchoolApproval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "TutorSchools",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            // Approve all existing schools (they existed before the approval feature)
            migrationBuilder.Sql("UPDATE \"TutorSchools\" SET \"IsApproved\" = true WHERE \"IsActive\" = true;");

            // Convert existing school-linked Tutor(1) users to SchoolTutor(4)
            migrationBuilder.Sql("UPDATE \"Users\" SET \"Role\" = 4 WHERE \"Role\" = 1 AND \"SchoolId\" IS NOT NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "TutorSchools");
        }
    }
}
