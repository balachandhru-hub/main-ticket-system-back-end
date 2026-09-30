using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class ConstraintsForDBValidation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "purchase_order_id",
                schema: "ticketsystem",
                table: "purchase_order_master",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "line_item",
                schema: "ticketsystem",
                table: "purchase_order_master",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "epid",
                schema: "ticketsystem",
                table: "purchase_order_master",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "line_item",
                schema: "ticketsystem",
                table: "purchase_order",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "epid",
                schema: "ticketsystem",
                table: "purchase_order",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "contract_id",
                schema: "ticketsystem",
                table: "ariba_contract",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_purchase_order_master_purchase_order_id_line_item_domain_id_epid",
                schema: "ticketsystem",
                table: "purchase_order_master",
                columns: new[] { "purchase_order_id", "line_item", "domain_id", "epid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_purchase_order_purchase_order_id_line_item_domain_id_epid",
                schema: "ticketsystem",
                table: "purchase_order",
                columns: new[] { "purchase_order_id", "line_item", "domain_id", "epid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_ariba_contract_contract_id_domain_id",
                schema: "ticketsystem",
                table: "ariba_contract",
                columns: new[] { "contract_id", "domain_id" },
                unique: true,
                filter: "[contract_id] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_purchase_order_master_purchase_order_id_line_item_domain_id_epid",
                schema: "ticketsystem",
                table: "purchase_order_master");

            migrationBuilder.DropIndex(
                name: "ix_purchase_order_purchase_order_id_line_item_domain_id_epid",
                schema: "ticketsystem",
                table: "purchase_order");

            migrationBuilder.DropIndex(
                name: "ix_ariba_contract_contract_id_domain_id",
                schema: "ticketsystem",
                table: "ariba_contract");

            migrationBuilder.AlterColumn<string>(
                name: "purchase_order_id",
                schema: "ticketsystem",
                table: "purchase_order_master",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "line_item",
                schema: "ticketsystem",
                table: "purchase_order_master",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "epid",
                schema: "ticketsystem",
                table: "purchase_order_master",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "line_item",
                schema: "ticketsystem",
                table: "purchase_order",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "epid",
                schema: "ticketsystem",
                table: "purchase_order",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "contract_id",
                schema: "ticketsystem",
                table: "ariba_contract",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
