using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class prAriba : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_ariba_prtasks_domain_id",
                schema: "ticketsystem",
                table: "ariba_prtasks");

            migrationBuilder.AlterColumn<string>(
                name: "requisition_id",
                schema: "ticketsystem",
                table: "purchase_order_master",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "glaccount",
                schema: "ticketsystem",
                table: "purchase_order_master",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "status",
                schema: "ticketsystem",
                table: "ariba_prtasks",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "requisitions_id",
                schema: "ticketsystem",
                table: "ariba_pr",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ariba_task_id",
                schema: "ticketsystem",
                table: "ariba_pr",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "ix_purchase_order_master_is_active_glaccount_requisition_id",
                schema: "ticketsystem",
                table: "purchase_order_master",
                columns: new[] { "is_active", "glaccount", "requisition_id" });

            migrationBuilder.CreateIndex(
                name: "ix_ariba_prtasks_domain_id_approver_date_updated",
                schema: "ticketsystem",
                table: "ariba_prtasks",
                columns: new[] { "domain_id", "approver", "date_updated" });

            migrationBuilder.CreateIndex(
                name: "ix_ariba_prtasks_domain_id_status_date_updated",
                schema: "ticketsystem",
                table: "ariba_prtasks",
                columns: new[] { "domain_id", "status", "date_updated" });

            migrationBuilder.CreateIndex(
                name: "ix_ariba_pr_is_active_ariba_task_id",
                schema: "ticketsystem",
                table: "ariba_pr",
                columns: new[] { "is_active", "ariba_task_id" });

            migrationBuilder.CreateIndex(
                name: "ix_ariba_pr_is_active_requisitions_date",
                schema: "ticketsystem",
                table: "ariba_pr",
                columns: new[] { "is_active", "requisitions_date" });

            migrationBuilder.CreateIndex(
                name: "ix_ariba_pr_is_active_requisitions_id",
                schema: "ticketsystem",
                table: "ariba_pr",
                columns: new[] { "is_active", "requisitions_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_purchase_order_master_is_active_glaccount_requisition_id",
                schema: "ticketsystem",
                table: "purchase_order_master");

            migrationBuilder.DropIndex(
                name: "ix_ariba_prtasks_domain_id_approver_date_updated",
                schema: "ticketsystem",
                table: "ariba_prtasks");

            migrationBuilder.DropIndex(
                name: "ix_ariba_prtasks_domain_id_status_date_updated",
                schema: "ticketsystem",
                table: "ariba_prtasks");

            migrationBuilder.DropIndex(
                name: "ix_ariba_pr_is_active_ariba_task_id",
                schema: "ticketsystem",
                table: "ariba_pr");

            migrationBuilder.DropIndex(
                name: "ix_ariba_pr_is_active_requisitions_date",
                schema: "ticketsystem",
                table: "ariba_pr");

            migrationBuilder.DropIndex(
                name: "ix_ariba_pr_is_active_requisitions_id",
                schema: "ticketsystem",
                table: "ariba_pr");

            migrationBuilder.AlterColumn<string>(
                name: "requisition_id",
                schema: "ticketsystem",
                table: "purchase_order_master",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "glaccount",
                schema: "ticketsystem",
                table: "purchase_order_master",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "status",
                schema: "ticketsystem",
                table: "ariba_prtasks",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "requisitions_id",
                schema: "ticketsystem",
                table: "ariba_pr",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "ariba_task_id",
                schema: "ticketsystem",
                table: "ariba_pr",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "ix_ariba_prtasks_domain_id",
                schema: "ticketsystem",
                table: "ariba_prtasks",
                column: "domain_id");
        }
    }
}
