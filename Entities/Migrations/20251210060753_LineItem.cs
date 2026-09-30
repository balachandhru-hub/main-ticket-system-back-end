using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class LineItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "line_item_number",
                schema: "ticketsystem",
                table: "material_pr",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "prunique_name",
                schema: "ticketsystem",
                table: "material_pr",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "line_item_number",
                schema: "ticketsystem",
                table: "material_pr");

            migrationBuilder.DropColumn(
                name: "prunique_name",
                schema: "ticketsystem",
                table: "material_pr");
        }
    }
}
