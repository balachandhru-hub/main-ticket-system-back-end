using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class PublicProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "public_property",
                schema: "ticketsystem",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    venue_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                    table.PrimaryKey("pk_public_property", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_public_property_is_active",
                schema: "ticketsystem",
                table: "public_property",
                column: "is_active");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "public_property",
                schema: "ticketsystem");
        }
    }
}
