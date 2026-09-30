using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class ServicePR : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "contract_details_id",
                schema: "ticketsystem",
                table: "contract_pending",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "service_pr",
                schema: "ticketsystem",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    domain_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    prnumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    purchase_org = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    commodity_code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    account_type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    delivery_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    uom = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    vendor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    contract = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    material_code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    service_start_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    service_end_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    max_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    expected_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    currency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    preparer_name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    glaccount = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    cost_center = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ship_to_plant = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    company_code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    purchase_group = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    bill_to = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tax_code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    deliver_to = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    error_comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    need_by = table.Column<DateTime>(type: "datetime2", nullable: false),
                    is_blocked = table.Column<bool>(type: "bit", nullable: false),
                    processed_state = table.Column<int>(type: "int", nullable: true),
                    line_item_number = table.Column<int>(type: "int", nullable: true),
                    prservice_unique_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    counter = table.Column<int>(type: "int", nullable: false),
                    preparer_password_adapter = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    date_created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    date_updated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_service_pr", x => x.id);
                    table.ForeignKey(
                        name: "fk_service_pr_domain_domain_id",
                        column: x => x.domain_id,
                        principalSchema: "ticketsystem",
                        principalTable: "domain",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "service_prlog",
                schema: "ticketsystem",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    batch_id = table.Column<int>(type: "int", nullable: false),
                    domain_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    prnumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    result = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    line_item_number = table.Column<int>(type: "int", nullable: true),
                    prservice_unique_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    date_created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    date_updated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_service_prlog", x => x.id);
                    table.ForeignKey(
                        name: "fk_service_prlog_domain_domain_id",
                        column: x => x.domain_id,
                        principalSchema: "ticketsystem",
                        principalTable: "domain",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_contract_pending_contract_details_id",
                schema: "ticketsystem",
                table: "contract_pending",
                column: "contract_details_id");

            migrationBuilder.CreateIndex(
                name: "ix_service_pr_domain_id",
                schema: "ticketsystem",
                table: "service_pr",
                column: "domain_id");

            migrationBuilder.CreateIndex(
                name: "ix_service_pr_is_active",
                schema: "ticketsystem",
                table: "service_pr",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_service_prlog_domain_id",
                schema: "ticketsystem",
                table: "service_prlog",
                column: "domain_id");

            migrationBuilder.CreateIndex(
                name: "ix_service_prlog_is_active",
                schema: "ticketsystem",
                table: "service_prlog",
                column: "is_active");

            migrationBuilder.AddForeignKey(
                name: "fk_contract_pending_contract_details_contract_details_id",
                schema: "ticketsystem",
                table: "contract_pending",
                column: "contract_details_id",
                principalSchema: "ticketsystem",
                principalTable: "contract_details",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_contract_pending_contract_details_contract_details_id",
                schema: "ticketsystem",
                table: "contract_pending");

            migrationBuilder.DropTable(
                name: "service_pr",
                schema: "ticketsystem");

            migrationBuilder.DropTable(
                name: "service_prlog",
                schema: "ticketsystem");

            migrationBuilder.DropIndex(
                name: "ix_contract_pending_contract_details_id",
                schema: "ticketsystem",
                table: "contract_pending");

            migrationBuilder.DropColumn(
                name: "contract_details_id",
                schema: "ticketsystem",
                table: "contract_pending");
        }
    }
}
