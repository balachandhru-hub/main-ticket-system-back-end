using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedAribaInvoice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_ariba_attachment_ariba_tasks_task_id",
                schema: "ticketsystem",
                table: "ariba_attachment");

            migrationBuilder.DropForeignKey(
                name: "fk_ariba_tasks_ariba_invoice_ariba_invoice_guid",
                schema: "ticketsystem",
                table: "ariba_tasks");

            migrationBuilder.DropIndex(
                name: "ix_ariba_tasks_ariba_invoice_guid",
                schema: "ticketsystem",
                table: "ariba_tasks");

            migrationBuilder.DropIndex(
                name: "ix_ariba_attachment_task_id",
                schema: "ticketsystem",
                table: "ariba_attachment");

            migrationBuilder.RenameColumn(
                name: "ariba_invoice_guid",
                schema: "ticketsystem",
                table: "ariba_tasks",
                newName: "approval_id");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                schema: "ticketsystem",
                table: "ariba_tasks",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<Guid>(
                name: "ariba_invoice_id",
                schema: "ticketsystem",
                table: "ariba_tasks",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "supplier_name",
                schema: "ticketsystem",
                table: "ariba_invoice",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                schema: "ticketsystem",
                table: "ariba_invoice",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<decimal>(
                name: "amount",
                schema: "ticketsystem",
                table: "ariba_invoice",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<Guid>(
                name: "ariba_tasks_id",
                schema: "ticketsystem",
                table: "ariba_attachment",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_ariba_tasks_ariba_invoice_id",
                schema: "ticketsystem",
                table: "ariba_tasks",
                column: "ariba_invoice_id");

            migrationBuilder.CreateIndex(
                name: "ix_ariba_invoice_domain_id",
                schema: "ticketsystem",
                table: "ariba_invoice",
                column: "domain_id");

            migrationBuilder.CreateIndex(
                name: "ix_ariba_attachment_ariba_tasks_id",
                schema: "ticketsystem",
                table: "ariba_attachment",
                column: "ariba_tasks_id");

            migrationBuilder.AddForeignKey(
                name: "fk_ariba_attachment_ariba_tasks_ariba_tasks_id",
                schema: "ticketsystem",
                table: "ariba_attachment",
                column: "ariba_tasks_id",
                principalSchema: "ticketsystem",
                principalTable: "ariba_tasks",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_ariba_invoice_domain_domain_id",
                schema: "ticketsystem",
                table: "ariba_invoice",
                column: "domain_id",
                principalSchema: "ticketsystem",
                principalTable: "domain",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_ariba_tasks_ariba_invoice_ariba_invoice_id",
                schema: "ticketsystem",
                table: "ariba_tasks",
                column: "ariba_invoice_id",
                principalSchema: "ticketsystem",
                principalTable: "ariba_invoice",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_ariba_attachment_ariba_tasks_ariba_tasks_id",
                schema: "ticketsystem",
                table: "ariba_attachment");

            migrationBuilder.DropForeignKey(
                name: "fk_ariba_invoice_domain_domain_id",
                schema: "ticketsystem",
                table: "ariba_invoice");

            migrationBuilder.DropForeignKey(
                name: "fk_ariba_tasks_ariba_invoice_ariba_invoice_id",
                schema: "ticketsystem",
                table: "ariba_tasks");

            migrationBuilder.DropIndex(
                name: "ix_ariba_tasks_ariba_invoice_id",
                schema: "ticketsystem",
                table: "ariba_tasks");

            migrationBuilder.DropIndex(
                name: "ix_ariba_invoice_domain_id",
                schema: "ticketsystem",
                table: "ariba_invoice");

            migrationBuilder.DropIndex(
                name: "ix_ariba_attachment_ariba_tasks_id",
                schema: "ticketsystem",
                table: "ariba_attachment");

            migrationBuilder.DropColumn(
                name: "ariba_invoice_id",
                schema: "ticketsystem",
                table: "ariba_tasks");

            migrationBuilder.DropColumn(
                name: "ariba_tasks_id",
                schema: "ticketsystem",
                table: "ariba_attachment");

            migrationBuilder.RenameColumn(
                name: "approval_id",
                schema: "ticketsystem",
                table: "ariba_tasks",
                newName: "ariba_invoice_guid");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                schema: "ticketsystem",
                table: "ariba_tasks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "supplier_name",
                schema: "ticketsystem",
                table: "ariba_invoice",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "description",
                schema: "ticketsystem",
                table: "ariba_invoice",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "amount",
                schema: "ticketsystem",
                table: "ariba_invoice",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_ariba_tasks_ariba_invoice_guid",
                schema: "ticketsystem",
                table: "ariba_tasks",
                column: "ariba_invoice_guid");

            migrationBuilder.CreateIndex(
                name: "ix_ariba_attachment_task_id",
                schema: "ticketsystem",
                table: "ariba_attachment",
                column: "task_id");

            migrationBuilder.AddForeignKey(
                name: "fk_ariba_attachment_ariba_tasks_task_id",
                schema: "ticketsystem",
                table: "ariba_attachment",
                column: "task_id",
                principalSchema: "ticketsystem",
                principalTable: "ariba_tasks",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_ariba_tasks_ariba_invoice_ariba_invoice_guid",
                schema: "ticketsystem",
                table: "ariba_tasks",
                column: "ariba_invoice_guid",
                principalSchema: "ticketsystem",
                principalTable: "ariba_invoice",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
