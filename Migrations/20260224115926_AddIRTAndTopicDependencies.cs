using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniStart.Migrations
{
    /// <inheritdoc />
    public partial class AddIRTAndTopicDependencies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Theta",
                table: "UserSkillProfiles",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ThetaSE",
                table: "UserSkillProfiles",
                type: "double precision",
                nullable: false,
                defaultValue: 1.0);

            migrationBuilder.AddColumn<double>(
                name: "DifficultyParam",
                table: "Questions",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "DiscriminationParam",
                table: "Questions",
                type: "double precision",
                nullable: false,
                defaultValue: 1.0);

            migrationBuilder.AddColumn<double>(
                name: "GuessParam",
                table: "Questions",
                type: "double precision",
                nullable: false,
                defaultValue: 0.25);

            migrationBuilder.CreateTable(
                name: "TopicDependencies",
                columns: table => new
                {
                    TopicId = table.Column<int>(type: "integer", nullable: false),
                    PrerequisiteTopicId = table.Column<int>(type: "integer", nullable: false),
                    Weight = table.Column<double>(type: "double precision", nullable: false, defaultValue: 1.0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TopicDependencies", x => new { x.TopicId, x.PrerequisiteTopicId });
                    table.ForeignKey(
                        name: "FK_TopicDependencies_Topics_PrerequisiteTopicId",
                        column: x => x.PrerequisiteTopicId,
                        principalTable: "Topics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TopicDependencies_Topics_TopicId",
                        column: x => x.TopicId,
                        principalTable: "Topics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TopicDependencies_PrerequisiteTopicId",
                table: "TopicDependencies",
                column: "PrerequisiteTopicId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TopicDependencies");

            migrationBuilder.DropColumn(
                name: "Theta",
                table: "UserSkillProfiles");

            migrationBuilder.DropColumn(
                name: "ThetaSE",
                table: "UserSkillProfiles");

            migrationBuilder.DropColumn(
                name: "DifficultyParam",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "DiscriminationParam",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "GuessParam",
                table: "Questions");
        }
    }
}
