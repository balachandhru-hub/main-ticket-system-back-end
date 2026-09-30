using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class DomainPlanUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "billing_cycle",
                schema: "ticketsystem",
                table: "plan_details");

            migrationBuilder.DropColumn(
                name: "payment_status",
                schema: "ticketsystem",
                table: "domain_plan");

            migrationBuilder.AddColumn<int>(
                name: "number_of_dues",
                schema: "ticketsystem",
                table: "domain_plan",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "cinnumber",
                schema: "ticketsystem",
                table: "domain",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "lutnumber",
                schema: "ticketsystem",
                table: "domain",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "number_of_dues",
                schema: "ticketsystem",
                table: "domain_plan");

            migrationBuilder.DropColumn(
                name: "cinnumber",
                schema: "ticketsystem",
                table: "domain");

            migrationBuilder.DropColumn(
                name: "lutnumber",
                schema: "ticketsystem",
                table: "domain");

            migrationBuilder.AddColumn<string>(
                name: "billing_cycle",
                schema: "ticketsystem",
                table: "plan_details",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "payment_status",
                schema: "ticketsystem",
                table: "domain_plan",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
