using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class PRLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "auto_gern",
                schema: "ticketsystem",
                table: "material_pr",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "auto_invoices",
                schema: "ticketsystem",
                table: "material_pr",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "invoice_date",
                schema: "ticketsystem",
                table: "material_pr",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "invoice_id",
                schema: "ticketsystem",
                table: "material_pr",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "recipt_id",
                schema: "ticketsystem",
                table: "material_pr",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "venue",
                schema: "ticketsystem",
                table: "material_pr",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "prlog",
                schema: "ticketsystem",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    batch_id = table.Column<int>(type: "int", nullable: false),
                    domain_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    purchase_order_id = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    prnumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    line_item_id = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    result = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    quantity = table.Column<double>(type: "float", nullable: false),
                    date_created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    date_updated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_prlog", x => x.id);
                    table.ForeignKey(
                        name: "fk_prlog_domain_domain_id",
                        column: x => x.domain_id,
                        principalSchema: "ticketsystem",
                        principalTable: "domain",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_prlog_domain_id",
                schema: "ticketsystem",
                table: "prlog",
                column: "domain_id");

            migrationBuilder.CreateIndex(
                name: "ix_prlog_is_active",
                schema: "ticketsystem",
                table: "prlog",
                column: "is_active");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "prlog",
                schema: "ticketsystem");

            migrationBuilder.DropColumn(
                name: "auto_gern",
                schema: "ticketsystem",
                table: "material_pr");

            migrationBuilder.DropColumn(
                name: "auto_invoices",
                schema: "ticketsystem",
                table: "material_pr");

            migrationBuilder.DropColumn(
                name: "invoice_date",
                schema: "ticketsystem",
                table: "material_pr");

            migrationBuilder.DropColumn(
                name: "invoice_id",
                schema: "ticketsystem",
                table: "material_pr");

            migrationBuilder.DropColumn(
                name: "recipt_id",
                schema: "ticketsystem",
                table: "material_pr");

            migrationBuilder.DropColumn(
                name: "venue",
                schema: "ticketsystem",
                table: "material_pr");
        }
    }
}
