using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class PaymentTableInvoiceData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "company_unique_name",
                schema: "ticketsystem",
                table: "ariba_payment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "contract_id",
                schema: "ticketsystem",
                table: "ariba_payment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "contract_value",
                schema: "ticketsystem",
                table: "ariba_payment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "description",
                schema: "ticketsystem",
                table: "ariba_payment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "remaining_balance",
                schema: "ticketsystem",
                table: "ariba_payment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "supplier_name",
                schema: "ticketsystem",
                table: "ariba_payment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "value_already_paid",
                schema: "ticketsystem",
                table: "ariba_payment",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "company_unique_name",
                schema: "ticketsystem",
                table: "ariba_payment");

            migrationBuilder.DropColumn(
                name: "contract_id",
                schema: "ticketsystem",
                table: "ariba_payment");

            migrationBuilder.DropColumn(
                name: "contract_value",
                schema: "ticketsystem",
                table: "ariba_payment");

            migrationBuilder.DropColumn(
                name: "description",
                schema: "ticketsystem",
                table: "ariba_payment");

            migrationBuilder.DropColumn(
                name: "remaining_balance",
                schema: "ticketsystem",
                table: "ariba_payment");

            migrationBuilder.DropColumn(
                name: "supplier_name",
                schema: "ticketsystem",
                table: "ariba_payment");

            migrationBuilder.DropColumn(
                name: "value_already_paid",
                schema: "ticketsystem",
                table: "ariba_payment");
        }
    }
}
