using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class aribainvoicecontractupdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "amount",
                schema: "ticketsystem",
                table: "ariba_contract_details",
                newName: "workspace_id");

            migrationBuilder.AddColumn<string>(
                name: "department",
                schema: "ticketsystem",
                table: "ariba_invoice",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "attachment_id",
                schema: "ticketsystem",
                table: "ariba_contract_details",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "department",
                schema: "ticketsystem",
                table: "ariba_invoice");

            migrationBuilder.DropColumn(
                name: "attachment_id",
                schema: "ticketsystem",
                table: "ariba_contract_details");

            migrationBuilder.RenameColumn(
                name: "workspace_id",
                schema: "ticketsystem",
                table: "ariba_contract_details",
                newName: "amount");
        }
    }
}
