using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class Password : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "line_item_number",
                schema: "ticketsystem",
                table: "material_prlog");

            migrationBuilder.DropColumn(
                name: "prunique_name",
                schema: "ticketsystem",
                table: "material_prlog");

            migrationBuilder.DropColumn(
                name: "quantity",
                schema: "ticketsystem",
                table: "material_prlog");

            migrationBuilder.AddColumn<string>(
                name: "requester_password_adapter",
                schema: "ticketsystem",
                table: "material_pr",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "requester_password_adapter",
                schema: "ticketsystem",
                table: "material_pr");

            migrationBuilder.AddColumn<int>(
                name: "line_item_number",
                schema: "ticketsystem",
                table: "material_prlog",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "prunique_name",
                schema: "ticketsystem",
                table: "material_prlog",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "quantity",
                schema: "ticketsystem",
                table: "material_prlog",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
