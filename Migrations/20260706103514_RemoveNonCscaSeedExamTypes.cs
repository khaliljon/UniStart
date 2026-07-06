using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace UniStart.Migrations
{
    /// <inheritdoc />
    public partial class RemoveNonCscaSeedExamTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remove legacy non-CSCA exam types, but ONLY the empty "shell" rows that
            // have no dependent data. This makes the migration safe to run on any
            // database: on a production DB where SAT/NUET still hold sections/questions/
            // attempts, the guard skips them (no FK failure) — the real data is removed
            // beforehand via DELETE /api/admin/exam-types/{code}. On a fresh DB the
            // seed shells are simply dropped.
            migrationBuilder.Sql(@"
                DELETE FROM ""ExamTypes"" e
                WHERE e.""Code"" IN ('SAT','NUET','IELTS','TOEFL')
                  AND NOT EXISTS (SELECT 1 FROM ""ExamSections"" s WHERE s.""ExamTypeCode"" = e.""Code"")
                  AND NOT EXISTS (SELECT 1 FROM ""MockExams"" m WHERE m.""ExamTypeCode"" = e.""Code"")
                  AND NOT EXISTS (SELECT 1 FROM ""TestSessions"" t WHERE t.""ExamTypeCode"" = e.""Code"")
                  AND NOT EXISTS (SELECT 1 FROM ""StudyGoals"" g WHERE g.""ExamTypeCode"" = e.""Code"");
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ExamTypes",
                columns: new[] { "Code", "Name" },
                values: new object[,]
                {
                    { "NUET", "NUET (Nazarbayev University Entrance Test)" },
                    { "SAT", "SAT (Scholastic Assessment Test)" }
                });
        }
    }
}
