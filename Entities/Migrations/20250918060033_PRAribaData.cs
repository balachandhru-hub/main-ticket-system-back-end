using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class PRAribaData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "prariba_data",
                schema: "ticketsystem",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    domain_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    prnumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    prtitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ariba_created_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ariba_submit_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ariba_approved_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ariba_ordered_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ariba_last_modified_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    preparer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    requester = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    company_code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    procurement_unit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    purchase_org = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    erp_po_number = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    total_cost = table.Column<double>(type: "float", nullable: true),
                    currency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    reporting_cost = table.Column<double>(type: "float", nullable: true),
                    tax_amount = table.Column<double>(type: "float", nullable: true),
                    tax_reporting = table.Column<double>(type: "float", nullable: true),
                    line_number = table.Column<int>(type: "int", nullable: true),
                    sap_line_number = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    item_description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    material_code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    commodity_code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    commodity_name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    quantity = table.Column<double>(type: "float", nullable: true),
                    uom = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    needed_by = table.Column<DateTime>(type: "datetime2", nullable: true),
                    unit_price = table.Column<double>(type: "float", nullable: true),
                    line_total = table.Column<double>(type: "float", nullable: true),
                    line_reporting = table.Column<double>(type: "float", nullable: true),
                    tax = table.Column<double>(type: "float", nullable: true),
                    supplier = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    supplier_part_number = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    cost_center = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    cost_center_desc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    gl_account = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    gl_description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    delivery_to = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    shipped_to = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    approver = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    approver_role = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    approval_state = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    approval_date = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    pending_approval_with = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    approved_by = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    date_created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    date_updated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_prariba_data", x => x.id);
                    table.ForeignKey(
                        name: "fk_prariba_data_domain_domain_id",
                        column: x => x.domain_id,
                        principalSchema: "ticketsystem",
                        principalTable: "domain",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_ariba_tasks_is_active",
                schema: "ticketsystem",
                table: "ariba_tasks",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_ariba_invoice_approval_history_is_active",
                schema: "ticketsystem",
                table: "ariba_invoice_approval_history",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_ariba_invoice_is_active",
                schema: "ticketsystem",
                table: "ariba_invoice",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_ariba_attachment_is_active",
                schema: "ticketsystem",
                table: "ariba_attachment",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_prariba_data_domain_id",
                schema: "ticketsystem",
                table: "prariba_data",
                column: "domain_id");

            migrationBuilder.CreateIndex(
                name: "ix_prariba_data_prnumber_is_active",
                schema: "ticketsystem",
                table: "prariba_data",
                columns: new[] { "prnumber", "is_active" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "prariba_data",
                schema: "ticketsystem");

            migrationBuilder.DropIndex(
                name: "ix_ariba_tasks_is_active",
                schema: "ticketsystem",
                table: "ariba_tasks");

            migrationBuilder.DropIndex(
                name: "ix_ariba_invoice_approval_history_is_active",
                schema: "ticketsystem",
                table: "ariba_invoice_approval_history");

            migrationBuilder.DropIndex(
                name: "ix_ariba_invoice_is_active",
                schema: "ticketsystem",
                table: "ariba_invoice");

            migrationBuilder.DropIndex(
                name: "ix_ariba_attachment_is_active",
                schema: "ticketsystem",
                table: "ariba_attachment");
        }
    }
}
