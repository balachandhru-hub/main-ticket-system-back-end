using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class invoiceupdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "task_id",
                schema: "ticketsystem",
                table: "ariba_attachment",
                newName: "ariba_task_id");

            migrationBuilder.AddColumn<DateTime>(
                name: "invoice_date",
                schema: "ticketsystem",
                table: "ariba_invoice",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "invoice_date",
                schema: "ticketsystem",
                table: "ariba_invoice");

            migrationBuilder.RenameColumn(
                name: "ariba_task_id",
                schema: "ticketsystem",
                table: "ariba_attachment",
                newName: "task_id");
        }
    }
}
