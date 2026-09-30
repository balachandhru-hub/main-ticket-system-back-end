using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class AddContactIdandContracStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "contract_id",
                schema: "ticketsystem",
                table: "easy_contract",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "contract_status",
                schema: "ticketsystem",
                table: "easy_contract",
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
                table: "easy_contract");

            migrationBuilder.DropColumn(
                name: "contract_status",
                schema: "ticketsystem",
                table: "easy_contract");
        }
    }
}
