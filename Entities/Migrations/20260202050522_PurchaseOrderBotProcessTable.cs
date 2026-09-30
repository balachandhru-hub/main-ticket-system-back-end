using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class PurchaseOrderBotProcessTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "purchase_order_bot_process",
                schema: "ticketsystem",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    domain_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    purchase_order_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    epid = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    line_item = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    unit_of_measure = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    material_code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    quantity = table.Column<double>(type: "float", nullable: false),
                    previous_accepted_quantity = table.Column<double>(type: "float", nullable: true),
                    receipt_id = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    processed_receipt_id = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    erpreceipt_id = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    additional_data = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    error_comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    is_blocked = table.Column<bool>(type: "bit", nullable: false),
                    success_cloud_erpid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    counter = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    processed_state = table.Column<int>(type: "int", nullable: true),
                    hard_reprocess = table.Column<bool>(type: "bit", nullable: false),
                    date_created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    date_updated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_purchase_order_bot_process", x => x.id);
                    table.ForeignKey(
                        name: "fk_purchase_order_bot_process_domain_domain_id",
                        column: x => x.domain_id,
                        principalSchema: "ticketsystem",
                        principalTable: "domain",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_purchase_order_bot_process_domain_id",
                schema: "ticketsystem",
                table: "purchase_order_bot_process",
                column: "domain_id");

            migrationBuilder.CreateIndex(
                name: "ix_purchase_order_bot_process_is_active",
                schema: "ticketsystem",
                table: "purchase_order_bot_process",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_purchase_order_bot_process_purchase_order_id",
                schema: "ticketsystem",
                table: "purchase_order_bot_process",
                column: "purchase_order_id");

            migrationBuilder.CreateIndex(
                name: "ix_purchase_order_bot_process_purchase_order_id_line_item_domain_id_epid",
                schema: "ticketsystem",
                table: "purchase_order_bot_process",
                columns: new[] { "purchase_order_id", "line_item", "domain_id", "epid" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "purchase_order_bot_process",
                schema: "ticketsystem");
        }
    }
}
