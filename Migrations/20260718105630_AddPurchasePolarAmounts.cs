using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniStart.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchasePolarAmounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "GrossAmount",
                table: "Purchases",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "NetAmount",
                table: "Purchases",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PlatformFeeAmount",
                table: "Purchases",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "PlatformFeeCurrency",
                table: "Purchases",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxAmount",
                table: "Purchases",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmount",
                table: "Purchases",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GrossAmount",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "NetAmount",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "PlatformFeeAmount",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "PlatformFeeCurrency",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "TaxAmount",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "TotalAmount",
                table: "Purchases");
        }
    }
}
