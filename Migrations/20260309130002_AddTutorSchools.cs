using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UniStart.Migrations
{
    /// <inheritdoc />
    public partial class AddTutorSchools : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SchoolId",
                table: "TutorProfiles",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TutorSchools",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    LogoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    WebsiteUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    InstagramUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TelegramUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Specializations = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsPartner = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TutorSchools", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TutorProfiles_SchoolId",
                table: "TutorProfiles",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_TutorSchools_Slug",
                table: "TutorSchools",
                column: "Slug",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TutorProfiles_TutorSchools_SchoolId",
                table: "TutorProfiles",
                column: "SchoolId",
                principalTable: "TutorSchools",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TutorProfiles_TutorSchools_SchoolId",
                table: "TutorProfiles");

            migrationBuilder.DropTable(
                name: "TutorSchools");

            migrationBuilder.DropIndex(
                name: "IX_TutorProfiles_SchoolId",
                table: "TutorProfiles");

            migrationBuilder.DropColumn(
                name: "SchoolId",
                table: "TutorProfiles");
        }
    }
}
