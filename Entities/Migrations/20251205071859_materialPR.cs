using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class materialPR : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "material_pr",
                schema: "ticketsystem",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    domain_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    purchase_org = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    commodity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    account_type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    delivery_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    uom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    vendor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    contract = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    currency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    requester_name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    glaccount = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    cost_center = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ship_to_plant = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    company_code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    purchase_group = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    bill_to = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    tax_code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    service_start_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    service_end_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    deliver_to = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    date_created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    date_updated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_material_pr", x => x.id);
                    table.ForeignKey(
                        name: "fk_material_pr_domain_domain_id",
                        column: x => x.domain_id,
                        principalSchema: "ticketsystem",
                        principalTable: "domain",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_material_pr_domain_id",
                schema: "ticketsystem",
                table: "material_pr",
                column: "domain_id");

            migrationBuilder.CreateIndex(
                name: "ix_material_pr_is_active",
                schema: "ticketsystem",
                table: "material_pr",
                column: "is_active");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "material_pr",
                schema: "ticketsystem");
        }
    }
}
