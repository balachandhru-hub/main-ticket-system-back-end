using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class InvoiceIDtoMaterialPRAttachment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "invoice_id",
                schema: "ticketsystem",
                table: "material_prattachment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "line_item_number",
                schema: "ticketsystem",
                table: "material_prattachment",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "invoice_id",
                schema: "ticketsystem",
                table: "material_prattachment");

            migrationBuilder.DropColumn(
                name: "line_item_number",
                schema: "ticketsystem",
                table: "material_prattachment");
        }
    }
}
