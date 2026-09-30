using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class FinalPurchaseOrderTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "line_item_number",
                schema: "ticketsystem",
                table: "material_prattachment",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "final_purchase_order",
                schema: "ticketsystem",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    domain_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    po_number = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    epid = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    line_item = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    quantity = table.Column<double>(type: "float", nullable: true),
                    amount = table.Column<double>(type: "float", nullable: true),
                    accepted = table.Column<double>(type: "float", nullable: true),
                    previous_accepted = table.Column<double>(type: "float", nullable: true),
                    ariba_receipt_id = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    s4_material_doc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    error_comments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    supplier = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    department = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    property = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    gl_account = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    cost_center = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    requester = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    result = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    date_created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    date_updated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_final_purchase_order", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_final_purchase_order_is_active",
                schema: "ticketsystem",
                table: "final_purchase_order",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_final_purchase_order_is_active_cost_center",
                schema: "ticketsystem",
                table: "final_purchase_order",
                columns: new[] { "is_active", "cost_center" });

            migrationBuilder.CreateIndex(
                name: "ix_final_purchase_order_is_active_department",
                schema: "ticketsystem",
                table: "final_purchase_order",
                columns: new[] { "is_active", "department" });

            migrationBuilder.CreateIndex(
                name: "ix_final_purchase_order_is_active_gl_account",
                schema: "ticketsystem",
                table: "final_purchase_order",
                columns: new[] { "is_active", "gl_account" });

            migrationBuilder.CreateIndex(
                name: "ix_final_purchase_order_is_active_po_number",
                schema: "ticketsystem",
                table: "final_purchase_order",
                columns: new[] { "is_active", "po_number" });

            migrationBuilder.CreateIndex(
                name: "ix_final_purchase_order_is_active_property",
                schema: "ticketsystem",
                table: "final_purchase_order",
                columns: new[] { "is_active", "property" });

            migrationBuilder.CreateIndex(
                name: "ix_final_purchase_order_is_active_requester",
                schema: "ticketsystem",
                table: "final_purchase_order",
                columns: new[] { "is_active", "requester" });

            migrationBuilder.CreateIndex(
                name: "ix_final_purchase_order_is_active_supplier",
                schema: "ticketsystem",
                table: "final_purchase_order",
                columns: new[] { "is_active", "supplier" });

            migrationBuilder.CreateIndex(
                name: "ix_final_purchase_order_is_active_supplier_department_property_gl_account_cost_center_requester_po_number",
                schema: "ticketsystem",
                table: "final_purchase_order",
                columns: new[] { "is_active", "supplier", "department", "property", "gl_account", "cost_center", "requester", "po_number" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "final_purchase_order",
                schema: "ticketsystem");

            migrationBuilder.AlterColumn<string>(
                name: "line_item_number",
                schema: "ticketsystem",
                table: "material_prattachment",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
