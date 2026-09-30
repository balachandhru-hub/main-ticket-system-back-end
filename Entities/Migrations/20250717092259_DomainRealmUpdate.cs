using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class DomainRealmUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ariba_realm_id",
                schema: "ticketsystem",
                table: "domain");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ariba_realm_id",
                schema: "ticketsystem",
                table: "domain",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
