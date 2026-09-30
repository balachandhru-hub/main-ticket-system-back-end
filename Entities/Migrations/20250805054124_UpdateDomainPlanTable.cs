using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDomainPlanTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "contract_asset_id",
                schema: "ticketsystem",
                table: "domain_plan",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "contract_end_date",
                schema: "ticketsystem",
                table: "domain_plan",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "contract_start_date",
                schema: "ticketsystem",
                table: "domain_plan",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "contract_asset_id",
                schema: "ticketsystem",
                table: "domain_plan");

            migrationBuilder.DropColumn(
                name: "contract_end_date",
                schema: "ticketsystem",
                table: "domain_plan");

            migrationBuilder.DropColumn(
                name: "contract_start_date",
                schema: "ticketsystem",
                table: "domain_plan");
        }
    }
}
