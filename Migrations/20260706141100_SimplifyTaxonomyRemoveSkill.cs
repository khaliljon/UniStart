using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UniStart.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyTaxonomyRemoveSkill : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Topics_ExamSections_SectionId",
                table: "Topics");

            migrationBuilder.DropForeignKey(
                name: "FK_Topics_Skills_SkillId",
                table: "Topics");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSkillProfiles_Skills_SkillId",
                table: "UserSkillProfiles");

            migrationBuilder.DropTable(
                name: "Skills");

            migrationBuilder.DropIndex(
                name: "IX_Topics_SkillId",
                table: "Topics");

            migrationBuilder.DropColumn(
                name: "SkillId",
                table: "Topics");

            migrationBuilder.RenameColumn(
                name: "SkillId",
                table: "UserSkillProfiles",
                newName: "SectionId");

            migrationBuilder.RenameIndex(
                name: "IX_UserSkillProfiles_SkillId",
                table: "UserSkillProfiles",
                newName: "IX_UserSkillProfiles_SectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Topics_ExamSections_SectionId",
                table: "Topics",
                column: "SectionId",
                principalTable: "ExamSections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserSkillProfiles_ExamSections_SectionId",
                table: "UserSkillProfiles",
                column: "SectionId",
                principalTable: "ExamSections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Topics_ExamSections_SectionId",
                table: "Topics");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSkillProfiles_ExamSections_SectionId",
                table: "UserSkillProfiles");

            migrationBuilder.RenameColumn(
                name: "SectionId",
                table: "UserSkillProfiles",
                newName: "SkillId");

            migrationBuilder.RenameIndex(
                name: "IX_UserSkillProfiles_SectionId",
                table: "UserSkillProfiles",
                newName: "IX_UserSkillProfiles_SkillId");

            migrationBuilder.AddColumn<int>(
                name: "SkillId",
                table: "Topics",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Skills",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Skills", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Topics_SkillId",
                table: "Topics",
                column: "SkillId");

            migrationBuilder.CreateIndex(
                name: "IX_Skills_Code",
                table: "Skills",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Topics_ExamSections_SectionId",
                table: "Topics",
                column: "SectionId",
                principalTable: "ExamSections",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Topics_Skills_SkillId",
                table: "Topics",
                column: "SkillId",
                principalTable: "Skills",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserSkillProfiles_Skills_SkillId",
                table: "UserSkillProfiles",
                column: "SkillId",
                principalTable: "Skills",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
