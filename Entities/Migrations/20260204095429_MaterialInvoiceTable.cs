using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class MaterialInvoiceTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "material_invoice",
                schema: "ticketsystem",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    prnumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    invoice_id = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    invoice_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    line_item_number = table.Column<int>(type: "int", nullable: true),
                    is_ocrattachment_processed = table.Column<bool>(type: "bit", nullable: false),
                    invoice_comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    is_header = table.Column<bool>(type: "bit", nullable: false),
                    date_created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    date_updated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_material_invoice", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_material_invoice_is_active",
                schema: "ticketsystem",
                table: "material_invoice",
                column: "is_active");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "material_invoice",
                schema: "ticketsystem");
        }
    }
}
