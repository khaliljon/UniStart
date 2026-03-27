using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniStart.Migrations
{
    /// <inheritdoc />
    public partial class SplitFirstNameLastName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            // Populate FirstName/LastName from existing Name column
            migrationBuilder.Sql(@"
                UPDATE ""Users""
                SET ""FirstName"" = CASE
                        WHEN POSITION(' ' IN ""Name"") > 0 THEN LEFT(""Name"", POSITION(' ' IN ""Name"") - 1)
                        ELSE ""Name""
                    END,
                    ""LastName"" = CASE
                        WHEN POSITION(' ' IN ""Name"") > 0 THEN SUBSTRING(""Name"" FROM POSITION(' ' IN ""Name"") + 1)
                        ELSE ''
                    END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Users");
        }
    }
}
