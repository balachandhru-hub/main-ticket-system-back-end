using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class RemovePhonenumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "notice_period",
                schema: "ticketsystem",
                table: "easy_contract");

            migrationBuilder.RenameColumn(
                name: "phone_number",
                schema: "ticketsystem",
                table: "easy_contract",
                newName: "description");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "description",
                schema: "ticketsystem",
                table: "easy_contract",
                newName: "phone_number");

            migrationBuilder.AddColumn<string>(
                name: "notice_period",
                schema: "ticketsystem",
                table: "easy_contract",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
