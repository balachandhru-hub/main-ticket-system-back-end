using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class AribaPRss : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "need_bydate",
                schema: "ticketsystem",
                table: "ariba_pr",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "prnet_amount",
                schema: "ticketsystem",
                table: "ariba_pr",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "prtax_aomunt",
                schema: "ticketsystem",
                table: "ariba_pr",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "need_bydate",
                schema: "ticketsystem",
                table: "ariba_pr");

            migrationBuilder.DropColumn(
                name: "prnet_amount",
                schema: "ticketsystem",
                table: "ariba_pr");

            migrationBuilder.DropColumn(
                name: "prtax_aomunt",
                schema: "ticketsystem",
                table: "ariba_pr");
        }
    }
}
