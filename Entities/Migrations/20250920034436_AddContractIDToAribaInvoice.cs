using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class AddContractIDToAribaInvoice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ponumber",
                schema: "ticketsystem",
                table: "ariba_tasks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "contract_id",
                schema: "ticketsystem",
                table: "ariba_invoice",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ponumber",
                schema: "ticketsystem",
                table: "ariba_tasks");

            migrationBuilder.DropColumn(
                name: "contract_id",
                schema: "ticketsystem",
                table: "ariba_invoice");
        }
    }
}
