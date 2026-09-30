using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class AribaInvoice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "invoice_net_amount",
                schema: "ticketsystem",
                table: "ariba_invoice",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "invoice_tax_amount",
                schema: "ticketsystem",
                table: "ariba_invoice",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "invoice_net_amount",
                schema: "ticketsystem",
                table: "ariba_invoice");

            migrationBuilder.DropColumn(
                name: "invoice_tax_amount",
                schema: "ticketsystem",
                table: "ariba_invoice");
        }
    }
}
