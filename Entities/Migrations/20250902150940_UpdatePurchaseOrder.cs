using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePurchaseOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "accepted_quantity",
                schema: "ticketsystem",
                table: "purchase_order");

            migrationBuilder.DropColumn(
                name: "previous_rejected_quantity",
                schema: "ticketsystem",
                table: "purchase_order");

            migrationBuilder.DropColumn(
                name: "rejected_quantity",
                schema: "ticketsystem",
                table: "purchase_order");

            migrationBuilder.AddColumn<int>(
                name: "counter",
                schema: "ticketsystem",
                table: "purchase_order",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "is_blocked",
                schema: "ticketsystem",
                table: "purchase_order",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "success_cloud_erpid",
                schema: "ticketsystem",
                table: "purchase_order",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "counter",
                schema: "ticketsystem",
                table: "purchase_order");

            migrationBuilder.DropColumn(
                name: "is_blocked",
                schema: "ticketsystem",
                table: "purchase_order");

            migrationBuilder.DropColumn(
                name: "success_cloud_erpid",
                schema: "ticketsystem",
                table: "purchase_order");

            migrationBuilder.AddColumn<double>(
                name: "accepted_quantity",
                schema: "ticketsystem",
                table: "purchase_order",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "previous_rejected_quantity",
                schema: "ticketsystem",
                table: "purchase_order",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "rejected_quantity",
                schema: "ticketsystem",
                table: "purchase_order",
                type: "float",
                nullable: true);
        }
    }
}
