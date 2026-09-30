using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedIndexOnAribaPRHistoryandAribaApprovalHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "requisitions_id",
                schema: "ticketsystem",
                table: "ariba_prhistory",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ariba_task_id",
                schema: "ticketsystem",
                table: "ariba_prhistory",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "invoice_id",
                schema: "ticketsystem",
                table: "ariba_invoice_approval_history",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ariba_task_id",
                schema: "ticketsystem",
                table: "ariba_invoice_approval_history",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "ix_ariba_prhistory_ariba_task_id_requisitions_id",
                schema: "ticketsystem",
                table: "ariba_prhistory",
                columns: new[] { "ariba_task_id", "requisitions_id" });

            migrationBuilder.CreateIndex(
                name: "ix_ariba_invoice_approval_history_ariba_task_id_invoice_id",
                schema: "ticketsystem",
                table: "ariba_invoice_approval_history",
                columns: new[] { "ariba_task_id", "invoice_id" });

            migrationBuilder.CreateIndex(
                name: "ix_ariba_contract_is_active",
                schema: "ticketsystem",
                table: "ariba_contract",
                column: "is_active");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_ariba_prhistory_ariba_task_id_requisitions_id",
                schema: "ticketsystem",
                table: "ariba_prhistory");

            migrationBuilder.DropIndex(
                name: "ix_ariba_invoice_approval_history_ariba_task_id_invoice_id",
                schema: "ticketsystem",
                table: "ariba_invoice_approval_history");

            migrationBuilder.DropIndex(
                name: "ix_ariba_contract_is_active",
                schema: "ticketsystem",
                table: "ariba_contract");

            migrationBuilder.AlterColumn<string>(
                name: "requisitions_id",
                schema: "ticketsystem",
                table: "ariba_prhistory",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "ariba_task_id",
                schema: "ticketsystem",
                table: "ariba_prhistory",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "invoice_id",
                schema: "ticketsystem",
                table: "ariba_invoice_approval_history",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "ariba_task_id",
                schema: "ticketsystem",
                table: "ariba_invoice_approval_history",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
