using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class ParentLineItemtoDraftMaterialPR : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "parent_line_item",
                schema: "ticketsystem",
                table: "draft_material_pr",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "percentage",
                schema: "ticketsystem",
                table: "draft_material_pr",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "parent_line_item",
                schema: "ticketsystem",
                table: "draft_material_pr");

            migrationBuilder.DropColumn(
                name: "percentage",
                schema: "ticketsystem",
                table: "draft_material_pr");
        }
    }
}
