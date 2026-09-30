using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class ContractHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "requisitions_id",
                schema: "ticketsystem",
                table: "ariba_contract_history",
                newName: "workspace_id");

            migrationBuilder.RenameColumn(
                name: "ariba_task_id",
                schema: "ticketsystem",
                table: "ariba_contract_history",
                newName: "task_id");

            migrationBuilder.AddColumn<string>(
                name: "contract_id",
                schema: "ticketsystem",
                table: "ariba_contract_history",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "contract_id",
                schema: "ticketsystem",
                table: "ariba_contract_history");

            migrationBuilder.RenameColumn(
                name: "workspace_id",
                schema: "ticketsystem",
                table: "ariba_contract_history",
                newName: "requisitions_id");

            migrationBuilder.RenameColumn(
                name: "task_id",
                schema: "ticketsystem",
                table: "ariba_contract_history",
                newName: "ariba_task_id");
        }
    }
}
