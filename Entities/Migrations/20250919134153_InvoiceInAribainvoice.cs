using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class InvoiceInAribainvoice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "approved_invoice_amount",
                schema: "ticketsystem",
                table: "ariba_invoice",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "company_unique_name",
                schema: "ticketsystem",
                table: "ariba_invoice",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "approved_invoice_amount",
                schema: "ticketsystem",
                table: "ariba_invoice");

            migrationBuilder.DropColumn(
                name: "company_unique_name",
                schema: "ticketsystem",
                table: "ariba_invoice");
        }
    }
}
