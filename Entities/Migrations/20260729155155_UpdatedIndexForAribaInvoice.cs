using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedIndexForAribaInvoice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_ariba_invoice_ariba_task_id",
                schema: "ticketsystem",
                table: "ariba_invoice",
                column: "ariba_task_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_ariba_invoice_ariba_task_id",
                schema: "ticketsystem",
                table: "ariba_invoice");
        }
    }
}
