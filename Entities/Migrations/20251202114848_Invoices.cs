using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class Invoices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_ariba_tasks_domain_id",
                schema: "ticketsystem",
                table: "ariba_tasks");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                schema: "ticketsystem",
                table: "ariba_tasks",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "supplier_name",
                schema: "ticketsystem",
                table: "ariba_invoice",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ponumber",
                schema: "ticketsystem",
                table: "ariba_invoice",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "invoice_number",
                schema: "ticketsystem",
                table: "ariba_invoice",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ariba_task_id",
                schema: "ticketsystem",
                table: "ariba_invoice",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "ix_purchase_order_master_is_active_glaccount_purchase_order_id",
                schema: "ticketsystem",
                table: "purchase_order_master",
                columns: new[] { "is_active", "glaccount", "purchase_order_id" });

            migrationBuilder.CreateIndex(
                name: "ix_ariba_tasks_domain_id_approver_date_updated",
                schema: "ticketsystem",
                table: "ariba_tasks",
                columns: new[] { "domain_id", "approver", "date_updated" });

            migrationBuilder.CreateIndex(
                name: "ix_ariba_tasks_domain_id_status_date_updated",
                schema: "ticketsystem",
                table: "ariba_tasks",
                columns: new[] { "domain_id", "status", "date_updated" });

            migrationBuilder.CreateIndex(
                name: "ix_ariba_invoice_is_active_ariba_task_id",
                schema: "ticketsystem",
                table: "ariba_invoice",
                columns: new[] { "is_active", "ariba_task_id" });

            migrationBuilder.CreateIndex(
                name: "ix_ariba_invoice_is_active_invoice_date",
                schema: "ticketsystem",
                table: "ariba_invoice",
                columns: new[] { "is_active", "invoice_date" });

            migrationBuilder.CreateIndex(
                name: "ix_ariba_invoice_is_active_invoice_number",
                schema: "ticketsystem",
                table: "ariba_invoice",
                columns: new[] { "is_active", "invoice_number" });

            migrationBuilder.CreateIndex(
                name: "ix_ariba_invoice_is_active_ponumber",
                schema: "ticketsystem",
                table: "ariba_invoice",
                columns: new[] { "is_active", "ponumber" });

            migrationBuilder.CreateIndex(
                name: "ix_ariba_invoice_is_active_supplier_name",
                schema: "ticketsystem",
                table: "ariba_invoice",
                columns: new[] { "is_active", "supplier_name" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_purchase_order_master_is_active_glaccount_purchase_order_id",
                schema: "ticketsystem",
                table: "purchase_order_master");

            migrationBuilder.DropIndex(
                name: "ix_ariba_tasks_domain_id_approver_date_updated",
                schema: "ticketsystem",
                table: "ariba_tasks");

            migrationBuilder.DropIndex(
                name: "ix_ariba_tasks_domain_id_status_date_updated",
                schema: "ticketsystem",
                table: "ariba_tasks");

            migrationBuilder.DropIndex(
                name: "ix_ariba_invoice_is_active_ariba_task_id",
                schema: "ticketsystem",
                table: "ariba_invoice");

            migrationBuilder.DropIndex(
                name: "ix_ariba_invoice_is_active_invoice_date",
                schema: "ticketsystem",
                table: "ariba_invoice");

            migrationBuilder.DropIndex(
                name: "ix_ariba_invoice_is_active_invoice_number",
                schema: "ticketsystem",
                table: "ariba_invoice");

            migrationBuilder.DropIndex(
                name: "ix_ariba_invoice_is_active_ponumber",
                schema: "ticketsystem",
                table: "ariba_invoice");

            migrationBuilder.DropIndex(
                name: "ix_ariba_invoice_is_active_supplier_name",
                schema: "ticketsystem",
                table: "ariba_invoice");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                schema: "ticketsystem",
                table: "ariba_tasks",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "supplier_name",
                schema: "ticketsystem",
                table: "ariba_invoice",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ponumber",
                schema: "ticketsystem",
                table: "ariba_invoice",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "invoice_number",
                schema: "ticketsystem",
                table: "ariba_invoice",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ariba_task_id",
                schema: "ticketsystem",
                table: "ariba_invoice",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "ix_ariba_tasks_domain_id",
                schema: "ticketsystem",
                table: "ariba_tasks",
                column: "domain_id");
        }
    }
}
