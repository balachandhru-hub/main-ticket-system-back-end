using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedPurchaseOrderMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "prnumber",
                schema: "ticketsystem",
                table: "purchase_order_master",
                newName: "unit_price_currency");

            migrationBuilder.AddColumn<string>(
                name: "account_category",
                schema: "ticketsystem",
                table: "purchase_order_master",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "asset_code",
                schema: "ticketsystem",
                table: "purchase_order_master",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "commodity_code",
                schema: "ticketsystem",
                table: "purchase_order_master",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "contract_id",
                schema: "ticketsystem",
                table: "purchase_order_master",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cost_center",
                schema: "ticketsystem",
                table: "purchase_order_master",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "erpcommodity_code",
                schema: "ticketsystem",
                table: "purchase_order_master",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "external_line_number",
                schema: "ticketsystem",
                table: "purchase_order_master",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "glaccount",
                schema: "ticketsystem",
                table: "purchase_order_master",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "item_category",
                schema: "ticketsystem",
                table: "purchase_order_master",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "line_type_category",
                schema: "ticketsystem",
                table: "purchase_order_master",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "need_by_date",
                schema: "ticketsystem",
                table: "purchase_order_master",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "parent_line_number",
                schema: "ticketsystem",
                table: "purchase_order_master",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "price",
                schema: "ticketsystem",
                table: "purchase_order_master",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "price_currency",
                schema: "ticketsystem",
                table: "purchase_order_master",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "purchase_group",
                schema: "ticketsystem",
                table: "purchase_order_master",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "purchase_org",
                schema: "ticketsystem",
                table: "purchase_order_master",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "requisition_id",
                schema: "ticketsystem",
                table: "purchase_order_master",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "requisition_requester",
                schema: "ticketsystem",
                table: "purchase_order_master",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "service_end_date",
                schema: "ticketsystem",
                table: "purchase_order_master",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "service_max_amount",
                schema: "ticketsystem",
                table: "purchase_order_master",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "service_start_date",
                schema: "ticketsystem",
                table: "purchase_order_master",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "tax_amount_in_base_currency",
                schema: "ticketsystem",
                table: "purchase_order_master",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tax_code",
                schema: "ticketsystem",
                table: "purchase_order_master",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "tax_percentage",
                schema: "ticketsystem",
                table: "purchase_order_master",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "unit_price",
                schema: "ticketsystem",
                table: "purchase_order_master",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "unit_price_in_base_price",
                schema: "ticketsystem",
                table: "purchase_order_master",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "version",
                schema: "ticketsystem",
                table: "purchase_order_master",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "account_category",
                schema: "ticketsystem",
                table: "purchase_order_master");

            migrationBuilder.DropColumn(
                name: "asset_code",
                schema: "ticketsystem",
                table: "purchase_order_master");

            migrationBuilder.DropColumn(
                name: "commodity_code",
                schema: "ticketsystem",
                table: "purchase_order_master");

            migrationBuilder.DropColumn(
                name: "contract_id",
                schema: "ticketsystem",
                table: "purchase_order_master");

            migrationBuilder.DropColumn(
                name: "cost_center",
                schema: "ticketsystem",
                table: "purchase_order_master");

            migrationBuilder.DropColumn(
                name: "erpcommodity_code",
                schema: "ticketsystem",
                table: "purchase_order_master");

            migrationBuilder.DropColumn(
                name: "external_line_number",
                schema: "ticketsystem",
                table: "purchase_order_master");

            migrationBuilder.DropColumn(
                name: "glaccount",
                schema: "ticketsystem",
                table: "purchase_order_master");

            migrationBuilder.DropColumn(
                name: "item_category",
                schema: "ticketsystem",
                table: "purchase_order_master");

            migrationBuilder.DropColumn(
                name: "line_type_category",
                schema: "ticketsystem",
                table: "purchase_order_master");

            migrationBuilder.DropColumn(
                name: "need_by_date",
                schema: "ticketsystem",
                table: "purchase_order_master");

            migrationBuilder.DropColumn(
                name: "parent_line_number",
                schema: "ticketsystem",
                table: "purchase_order_master");

            migrationBuilder.DropColumn(
                name: "price",
                schema: "ticketsystem",
                table: "purchase_order_master");

            migrationBuilder.DropColumn(
                name: "price_currency",
                schema: "ticketsystem",
                table: "purchase_order_master");

            migrationBuilder.DropColumn(
                name: "purchase_group",
                schema: "ticketsystem",
                table: "purchase_order_master");

            migrationBuilder.DropColumn(
                name: "purchase_org",
                schema: "ticketsystem",
                table: "purchase_order_master");

            migrationBuilder.DropColumn(
                name: "requisition_id",
                schema: "ticketsystem",
                table: "purchase_order_master");

            migrationBuilder.DropColumn(
                name: "requisition_requester",
                schema: "ticketsystem",
                table: "purchase_order_master");

            migrationBuilder.DropColumn(
                name: "service_end_date",
                schema: "ticketsystem",
                table: "purchase_order_master");

            migrationBuilder.DropColumn(
                name: "service_max_amount",
                schema: "ticketsystem",
                table: "purchase_order_master");

            migrationBuilder.DropColumn(
                name: "service_start_date",
                schema: "ticketsystem",
                table: "purchase_order_master");

            migrationBuilder.DropColumn(
                name: "tax_amount_in_base_currency",
                schema: "ticketsystem",
                table: "purchase_order_master");

            migrationBuilder.DropColumn(
                name: "tax_code",
                schema: "ticketsystem",
                table: "purchase_order_master");

            migrationBuilder.DropColumn(
                name: "tax_percentage",
                schema: "ticketsystem",
                table: "purchase_order_master");

            migrationBuilder.DropColumn(
                name: "unit_price",
                schema: "ticketsystem",
                table: "purchase_order_master");

            migrationBuilder.DropColumn(
                name: "unit_price_in_base_price",
                schema: "ticketsystem",
                table: "purchase_order_master");

            migrationBuilder.DropColumn(
                name: "version",
                schema: "ticketsystem",
                table: "purchase_order_master");

            migrationBuilder.RenameColumn(
                name: "unit_price_currency",
                schema: "ticketsystem",
                table: "purchase_order_master",
                newName: "prnumber");
        }
    }
}
