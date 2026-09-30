using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePOTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "unit_of_measure",
                schema: "ticketsystem",
                table: "purchase_order",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "material_code",
                schema: "ticketsystem",
                table: "purchase_order",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "erpreceipt_id",
                schema: "ticketsystem",
                table: "purchase_order",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "processed_receipt_id",
                schema: "ticketsystem",
                table: "purchase_order",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "receipt_id",
                schema: "ticketsystem",
                table: "purchase_order",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "erpreceipt_id",
                schema: "ticketsystem",
                table: "purchase_order");

            migrationBuilder.DropColumn(
                name: "processed_receipt_id",
                schema: "ticketsystem",
                table: "purchase_order");

            migrationBuilder.DropColumn(
                name: "receipt_id",
                schema: "ticketsystem",
                table: "purchase_order");

            migrationBuilder.AlterColumn<string>(
                name: "unit_of_measure",
                schema: "ticketsystem",
                table: "purchase_order",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "material_code",
                schema: "ticketsystem",
                table: "purchase_order",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
