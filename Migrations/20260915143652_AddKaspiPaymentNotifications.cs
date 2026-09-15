using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UniStart.Migrations
{
    /// <inheritdoc />
    public partial class AddKaspiPaymentNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KaspiPaymentNotifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Source = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Gmail"),
                    SourceEventId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    KaspiPaymentId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OrderCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    Currency = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false, defaultValue: "KZT"),
                    PaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReceivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Detected"),
                    PaymentOrderId = table.Column<int>(type: "integer", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KaspiPaymentNotifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KaspiPaymentNotifications_PaymentOrders_PaymentOrderId",
                        column: x => x.PaymentOrderId,
                        principalTable: "PaymentOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KaspiPaymentNotifications_KaspiPaymentId_Trusted",
                table: "KaspiPaymentNotifications",
                column: "KaspiPaymentId",
                unique: true,
                filter: "\"KaspiPaymentId\" IS NOT NULL AND \"Status\" IN ('Matched', 'Processed')");

            migrationBuilder.CreateIndex(
                name: "IX_KaspiPaymentNotifications_OrderCode",
                table: "KaspiPaymentNotifications",
                column: "OrderCode");

            migrationBuilder.CreateIndex(
                name: "IX_KaspiPaymentNotifications_PaymentOrderId",
                table: "KaspiPaymentNotifications",
                column: "PaymentOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_KaspiPaymentNotifications_Source_SourceEventId",
                table: "KaspiPaymentNotifications",
                columns: new[] { "Source", "SourceEventId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KaspiPaymentNotifications_Status",
                table: "KaspiPaymentNotifications",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KaspiPaymentNotifications");
        }
    }
}
