using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniStart.Migrations
{
    /// <inheritdoc />
    public partial class AddMockAttemptIsFreeAndPurchaseCheckoutRef : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CheckoutRef",
                table: "Purchases",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFree",
                table: "MockExamAttempts",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CheckoutRef",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "IsFree",
                table: "MockExamAttempts");
        }
    }
}
