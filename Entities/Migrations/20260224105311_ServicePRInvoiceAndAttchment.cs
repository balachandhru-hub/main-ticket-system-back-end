using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class ServicePRInvoiceAndAttchment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "auto_invoices",
                schema: "ticketsystem",
                table: "service_pr",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "auto_ses",
                schema: "ticketsystem",
                table: "service_pr",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "category",
                schema: "ticketsystem",
                table: "service_pr",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "parent_line_item_number",
                schema: "ticketsystem",
                table: "service_pr",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ponumber",
                schema: "ticketsystem",
                table: "service_pr",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "tax_include",
                schema: "ticketsystem",
                table: "service_pr",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "vendor_name",
                schema: "ticketsystem",
                table: "service_pr",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "service_prattachment",
                schema: "ticketsystem",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    prservice_unique_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    asset_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    invoice_id = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    line_item_number = table.Column<int>(type: "int", nullable: true),
                    date_created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    date_updated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_service_prattachment", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "service_prinvoice",
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
                    table.PrimaryKey("pk_service_prinvoice", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_service_prattachment_is_active",
                schema: "ticketsystem",
                table: "service_prattachment",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_service_prinvoice_is_active",
                schema: "ticketsystem",
                table: "service_prinvoice",
                column: "is_active");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "service_prattachment",
                schema: "ticketsystem");

            migrationBuilder.DropTable(
                name: "service_prinvoice",
                schema: "ticketsystem");

            migrationBuilder.DropColumn(
                name: "auto_invoices",
                schema: "ticketsystem",
                table: "service_pr");

            migrationBuilder.DropColumn(
                name: "auto_ses",
                schema: "ticketsystem",
                table: "service_pr");

            migrationBuilder.DropColumn(
                name: "category",
                schema: "ticketsystem",
                table: "service_pr");

            migrationBuilder.DropColumn(
                name: "parent_line_item_number",
                schema: "ticketsystem",
                table: "service_pr");

            migrationBuilder.DropColumn(
                name: "ponumber",
                schema: "ticketsystem",
                table: "service_pr");

            migrationBuilder.DropColumn(
                name: "tax_include",
                schema: "ticketsystem",
                table: "service_pr");

            migrationBuilder.DropColumn(
                name: "vendor_name",
                schema: "ticketsystem",
                table: "service_pr");
        }
    }
}
