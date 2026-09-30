using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class PropertyCrossCharge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "percentage",
                schema: "ticketsystem",
                table: "material_pr");

            migrationBuilder.CreateTable(
                name: "property_cross_charge",
                schema: "ticketsystem",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    property_cross_charge_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    company_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    domain_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    company_code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    cost_center = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    glaccount = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ship_to_plant = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    purchase_org = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    purchase_group = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    bill_to = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    date_created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    date_updated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_property_cross_charge", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_property_cross_charge_is_active",
                schema: "ticketsystem",
                table: "property_cross_charge",
                column: "is_active");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "property_cross_charge",
                schema: "ticketsystem");

            migrationBuilder.AddColumn<double>(
                name: "percentage",
                schema: "ticketsystem",
                table: "material_pr",
                type: "float",
                nullable: true);
        }
    }
}
