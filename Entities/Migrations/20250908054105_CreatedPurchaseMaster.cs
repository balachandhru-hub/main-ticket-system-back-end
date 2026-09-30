using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class CreatedPurchaseMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "purchase_order_master",
                schema: "ticketsystem",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    purchase_order_id = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    epid = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    line_item = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    quantity = table.Column<double>(type: "float", nullable: false),
                    unit_of_measure = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    material_code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    prnumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    company_code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    supplier_id = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    supplier_name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    date_created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    date_updated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_purchase_order_master", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_purchase_order_master_is_active",
                schema: "ticketsystem",
                table: "purchase_order_master",
                column: "is_active");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "purchase_order_master",
                schema: "ticketsystem");
        }
    }
}
