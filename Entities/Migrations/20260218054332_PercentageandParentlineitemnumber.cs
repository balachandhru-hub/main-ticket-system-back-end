using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class PercentageandParentlineitemnumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "parent_line_item_number",
                schema: "ticketsystem",
                table: "material_pr",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "percentage",
                schema: "ticketsystem",
                table: "material_pr",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "parent_line_item_number",
                schema: "ticketsystem",
                table: "material_pr");

            migrationBuilder.DropColumn(
                name: "percentage",
                schema: "ticketsystem",
                table: "material_pr");
        }
    }
}
