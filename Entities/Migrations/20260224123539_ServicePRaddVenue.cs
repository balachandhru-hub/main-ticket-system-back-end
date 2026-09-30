using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class ServicePRaddVenue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "header_comment",
                schema: "ticketsystem",
                table: "service_pr",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "invoice_comment",
                schema: "ticketsystem",
                table: "service_pr",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "invoice_date",
                schema: "ticketsystem",
                table: "service_pr",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "invoice_id",
                schema: "ticketsystem",
                table: "service_pr",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_ocrattachment_processed",
                schema: "ticketsystem",
                table: "service_pr",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "venue",
                schema: "ticketsystem",
                table: "service_pr",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "header_comment",
                schema: "ticketsystem",
                table: "service_pr");

            migrationBuilder.DropColumn(
                name: "invoice_comment",
                schema: "ticketsystem",
                table: "service_pr");

            migrationBuilder.DropColumn(
                name: "invoice_date",
                schema: "ticketsystem",
                table: "service_pr");

            migrationBuilder.DropColumn(
                name: "invoice_id",
                schema: "ticketsystem",
                table: "service_pr");

            migrationBuilder.DropColumn(
                name: "is_ocrattachment_processed",
                schema: "ticketsystem",
                table: "service_pr");

            migrationBuilder.DropColumn(
                name: "venue",
                schema: "ticketsystem",
                table: "service_pr");
        }
    }
}
