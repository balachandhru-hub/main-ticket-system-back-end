using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class RemovePercentageFromDraftMaterialPR : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "percentage",
                schema: "ticketsystem",
                table: "draft_material_pr");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "percentage",
                schema: "ticketsystem",
                table: "draft_material_pr",
                type: "float",
                nullable: true);
        }
    }
}
