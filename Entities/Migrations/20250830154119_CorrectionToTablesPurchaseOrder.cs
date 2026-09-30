using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class CorrectionToTablesPurchaseOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "domain_settings",
                schema: "ticketsystem");

            migrationBuilder.DropTable(
                name: "purchase_orders",
                schema: "ticketsystem");

            migrationBuilder.CreateTable(
                name: "domain_setting",
                schema: "ticketsystem",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    domain_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    settings_key = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    settings_value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    date_created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    date_updated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_domain_setting", x => x.id);
                    table.ForeignKey(
                        name: "fk_domain_setting_domain_domain_id",
                        column: x => x.domain_id,
                        principalSchema: "ticketsystem",
                        principalTable: "domain",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "purchase_order",
                schema: "ticketsystem",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    domain_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    purchase_order_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    epid = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    line_item = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    unit_of_measure = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    material_code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    quantity = table.Column<double>(type: "float", nullable: false),
                    accepted_quantity = table.Column<double>(type: "float", nullable: true),
                    rejected_quantity = table.Column<double>(type: "float", nullable: true),
                    previous_accepted_quantity = table.Column<double>(type: "float", nullable: true),
                    previous_rejected_quantity = table.Column<double>(type: "float", nullable: true),
                    additional_data = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    date_created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    date_updated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_purchase_order", x => x.id);
                    table.ForeignKey(
                        name: "fk_purchase_order_domain_domain_id",
                        column: x => x.domain_id,
                        principalSchema: "ticketsystem",
                        principalTable: "domain",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_domain_setting_domain_id",
                schema: "ticketsystem",
                table: "domain_setting",
                column: "domain_id");

            migrationBuilder.CreateIndex(
                name: "ix_domain_setting_is_active",
                schema: "ticketsystem",
                table: "domain_setting",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_purchase_order_domain_id",
                schema: "ticketsystem",
                table: "purchase_order",
                column: "domain_id");

            migrationBuilder.CreateIndex(
                name: "ix_purchase_order_is_active",
                schema: "ticketsystem",
                table: "purchase_order",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_purchase_order_purchase_order_id",
                schema: "ticketsystem",
                table: "purchase_order",
                column: "purchase_order_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "domain_setting",
                schema: "ticketsystem");

            migrationBuilder.DropTable(
                name: "purchase_order",
                schema: "ticketsystem");

            migrationBuilder.CreateTable(
                name: "domain_settings",
                schema: "ticketsystem",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    domain_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    date_created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    date_updated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    settings_key = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    settings_value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_domain_settings", x => x.id);
                    table.ForeignKey(
                        name: "fk_domain_settings_domain_domain_id",
                        column: x => x.domain_id,
                        principalSchema: "ticketsystem",
                        principalTable: "domain",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "purchase_orders",
                schema: "ticketsystem",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    domain_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    accepted_quantity = table.Column<double>(type: "float", nullable: true),
                    additional_data = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    date_created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    date_updated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    epid = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    line_item = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    material_code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    previous_accepted_quantity = table.Column<double>(type: "float", nullable: true),
                    previous_rejected_quantity = table.Column<double>(type: "float", nullable: true),
                    purchase_order_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    quantity = table.Column<double>(type: "float", nullable: true),
                    rejected_quantity = table.Column<double>(type: "float", nullable: true),
                    unit_of_material = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_purchase_orders", x => x.id);
                    table.ForeignKey(
                        name: "fk_purchase_orders_domain_domain_id",
                        column: x => x.domain_id,
                        principalSchema: "ticketsystem",
                        principalTable: "domain",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_domain_settings_domain_id",
                schema: "ticketsystem",
                table: "domain_settings",
                column: "domain_id");

            migrationBuilder.CreateIndex(
                name: "ix_domain_settings_is_active",
                schema: "ticketsystem",
                table: "domain_settings",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_purchase_orders_domain_id",
                schema: "ticketsystem",
                table: "purchase_orders",
                column: "domain_id");

            migrationBuilder.CreateIndex(
                name: "ix_purchase_orders_is_active",
                schema: "ticketsystem",
                table: "purchase_orders",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_purchase_orders_purchase_order_id",
                schema: "ticketsystem",
                table: "purchase_orders",
                column: "purchase_order_id");
        }
    }
}
