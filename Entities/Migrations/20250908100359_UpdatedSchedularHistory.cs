using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedSchedularHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "domain_id",
                schema: "ticketsystem",
                table: "schedular_history",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "domain_id",
                schema: "ticketsystem",
                table: "purchase_order_master",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "ix_schedular_history_domain_id",
                schema: "ticketsystem",
                table: "schedular_history",
                column: "domain_id");

            migrationBuilder.CreateIndex(
                name: "ix_purchase_order_master_domain_id",
                schema: "ticketsystem",
                table: "purchase_order_master",
                column: "domain_id");

            migrationBuilder.AddForeignKey(
                name: "fk_purchase_order_master_domain_domain_id",
                schema: "ticketsystem",
                table: "purchase_order_master",
                column: "domain_id",
                principalSchema: "ticketsystem",
                principalTable: "domain",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_schedular_history_domain_domain_id",
                schema: "ticketsystem",
                table: "schedular_history",
                column: "domain_id",
                principalSchema: "ticketsystem",
                principalTable: "domain",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_purchase_order_master_domain_domain_id",
                schema: "ticketsystem",
                table: "purchase_order_master");

            migrationBuilder.DropForeignKey(
                name: "fk_schedular_history_domain_domain_id",
                schema: "ticketsystem",
                table: "schedular_history");

            migrationBuilder.DropIndex(
                name: "ix_schedular_history_domain_id",
                schema: "ticketsystem",
                table: "schedular_history");

            migrationBuilder.DropIndex(
                name: "ix_purchase_order_master_domain_id",
                schema: "ticketsystem",
                table: "purchase_order_master");

            migrationBuilder.DropColumn(
                name: "domain_id",
                schema: "ticketsystem",
                table: "schedular_history");

            migrationBuilder.DropColumn(
                name: "domain_id",
                schema: "ticketsystem",
                table: "purchase_order_master");
        }
    }
}
