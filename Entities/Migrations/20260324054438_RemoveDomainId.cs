using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDomainId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_ariba_currency_domain_domain_id",
                schema: "ticketsystem",
                table: "ariba_currency");

            migrationBuilder.DropIndex(
                name: "ix_ariba_currency_domain_id",
                schema: "ticketsystem",
                table: "ariba_currency");

            migrationBuilder.DropColumn(
                name: "domain_id",
                schema: "ticketsystem",
                table: "ariba_currency");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "domain_id",
                schema: "ticketsystem",
                table: "ariba_currency",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "ix_ariba_currency_domain_id",
                schema: "ticketsystem",
                table: "ariba_currency",
                column: "domain_id");

            migrationBuilder.AddForeignKey(
                name: "fk_ariba_currency_domain_domain_id",
                schema: "ticketsystem",
                table: "ariba_currency",
                column: "domain_id",
                principalSchema: "ticketsystem",
                principalTable: "domain",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
