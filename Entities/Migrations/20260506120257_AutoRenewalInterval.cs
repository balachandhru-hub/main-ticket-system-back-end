using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class AutoRenewalInterval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "auto_renewal_interval",
                schema: "ticketsystem",
                table: "easy_contract",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "maximum_number_of_renewals",
                schema: "ticketsystem",
                table: "easy_contract",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "auto_renewal_interval",
                schema: "ticketsystem",
                table: "easy_contract");

            migrationBuilder.DropColumn(
                name: "maximum_number_of_renewals",
                schema: "ticketsystem",
                table: "easy_contract");
        }
    }
}
