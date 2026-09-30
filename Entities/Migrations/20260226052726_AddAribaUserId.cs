using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class AddAribaUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ariba_user_id",
                schema: "ticketsystem",
                table: "material_pr",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ariba_user_id",
                schema: "ticketsystem",
                table: "draft_material_pr",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ariba_user_id",
                schema: "ticketsystem",
                table: "ariba_venue_list_mapping",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ariba_user_id",
                schema: "ticketsystem",
                table: "material_pr");

            migrationBuilder.DropColumn(
                name: "ariba_user_id",
                schema: "ticketsystem",
                table: "draft_material_pr");

            migrationBuilder.DropColumn(
                name: "ariba_user_id",
                schema: "ticketsystem",
                table: "ariba_venue_list_mapping");
        }
    }
}
