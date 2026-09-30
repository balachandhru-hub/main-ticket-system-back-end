using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class DraftMaterialPRTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "draft_material_pr",
                schema: "ticketsystem",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    domain_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    purchase_org = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    draft_id = table.Column<int>(type: "int", nullable: false),
                    commodity_code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    account_type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    delivery_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    uom = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    vendor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    contract = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    material_code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    currency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    requester_name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    glaccount = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    cost_center = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ship_to_plant = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    company_code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    purchase_group = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    bill_to = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tax_code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    header_comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    deliver_to = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    error_comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    need_by = table.Column<DateTime>(type: "datetime2", nullable: false),
                    auto_grn = table.Column<bool>(type: "bit", nullable: false),
                    auto_invoices = table.Column<bool>(type: "bit", nullable: false),
                    tax_include = table.Column<bool>(type: "bit", nullable: false),
                    venue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    recipt_id = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    invoice_id = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    invoice_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    is_blocked = table.Column<bool>(type: "bit", nullable: false),
                    processed_state = table.Column<int>(type: "int", nullable: true),
                    line_item_number = table.Column<int>(type: "int", nullable: true),
                    prunique_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    counter = table.Column<int>(type: "int", nullable: false),
                    ponumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    requester_password_adapter = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    vendor_name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    is_ocrattachment_processed = table.Column<bool>(type: "bit", nullable: false),
                    invoice_comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    date_created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    date_updated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_draft_material_pr", x => x.id);
                    table.ForeignKey(
                        name: "fk_draft_material_pr_domain_domain_id",
                        column: x => x.domain_id,
                        principalSchema: "ticketsystem",
                        principalTable: "domain",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_draft_material_pr_domain_id",
                schema: "ticketsystem",
                table: "draft_material_pr",
                column: "domain_id");

            migrationBuilder.CreateIndex(
                name: "ix_draft_material_pr_is_active",
                schema: "ticketsystem",
                table: "draft_material_pr",
                column: "is_active");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "draft_material_pr",
                schema: "ticketsystem");
        }
    }
}
