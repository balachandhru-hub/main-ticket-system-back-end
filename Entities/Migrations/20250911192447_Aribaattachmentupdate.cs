using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class Aribaattachmentupdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "file_bytes",
                schema: "ticketsystem",
                table: "ariba_attachment");

            migrationBuilder.DropColumn(
                name: "task_id",
                schema: "ticketsystem",
                table: "ariba_attachment");

            migrationBuilder.RenameColumn(
                name: "file_name",
                schema: "ticketsystem",
                table: "ariba_attachment",
                newName: "invoice_id");

            migrationBuilder.RenameColumn(
                name: "content_type",
                schema: "ticketsystem",
                table: "ariba_attachment",
                newName: "attachment_id");

            migrationBuilder.AlterColumn<string>(
                name: "approval_id",
                schema: "ticketsystem",
                table: "ariba_tasks",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "domain_id",
                schema: "ticketsystem",
                table: "ariba_tasks",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "ix_ariba_tasks_domain_id",
                schema: "ticketsystem",
                table: "ariba_tasks",
                column: "domain_id");

            migrationBuilder.AddForeignKey(
                name: "fk_ariba_tasks_domain_domain_id",
                schema: "ticketsystem",
                table: "ariba_tasks",
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
                name: "fk_ariba_tasks_domain_domain_id",
                schema: "ticketsystem",
                table: "ariba_tasks");

            migrationBuilder.DropIndex(
                name: "ix_ariba_tasks_domain_id",
                schema: "ticketsystem",
                table: "ariba_tasks");

            migrationBuilder.DropColumn(
                name: "domain_id",
                schema: "ticketsystem",
                table: "ariba_tasks");

            migrationBuilder.RenameColumn(
                name: "invoice_id",
                schema: "ticketsystem",
                table: "ariba_attachment",
                newName: "file_name");

            migrationBuilder.RenameColumn(
                name: "attachment_id",
                schema: "ticketsystem",
                table: "ariba_attachment",
                newName: "content_type");

            migrationBuilder.AlterColumn<Guid>(
                name: "approval_id",
                schema: "ticketsystem",
                table: "ariba_tasks",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<byte[]>(
                name: "file_bytes",
                schema: "ticketsystem",
                table: "ariba_attachment",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<Guid>(
                name: "task_id",
                schema: "ticketsystem",
                table: "ariba_attachment",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }
    }
}
