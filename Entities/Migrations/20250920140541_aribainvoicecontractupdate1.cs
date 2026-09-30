using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class aribainvoicecontractupdate1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "approved_invoice_amount",
                schema: "ticketsystem",
                table: "ariba_invoice");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "approved_invoice_amount",
                schema: "ticketsystem",
                table: "ariba_invoice",
                type: "decimal(18,2)",
                nullable: true);
        }
    }
}
