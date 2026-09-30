using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedNecessaryIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_ticket_is_active",
                schema: "ticketsystem",
                table: "ticket");

            migrationBuilder.DropIndex(
                name: "ix_purchase_order_master_is_active",
                schema: "ticketsystem",
                table: "purchase_order_master");

            migrationBuilder.DropIndex(
                name: "ix_asset_is_active",
                schema: "ticketsystem",
                table: "asset");

            migrationBuilder.DropIndex(
                name: "ix_ariba_tasks_is_active",
                schema: "ticketsystem",
                table: "ariba_tasks");

            migrationBuilder.DropIndex(
                name: "ix_ariba_prtasks_is_active",
                schema: "ticketsystem",
                table: "ariba_prtasks");

            migrationBuilder.DropIndex(
                name: "ix_access_token_is_active",
                schema: "ticketsystem",
                table: "access_token");

            migrationBuilder.CreateIndex(
                name: "ix_ticket_is_active_domain_id",
                schema: "ticketsystem",
                table: "ticket",
                columns: new[] { "is_active", "domain_id" })
                .Annotation("SqlServer:Include", new[] { "status" });

            migrationBuilder.CreateIndex(
                name: "ix_purchase_order_master_is_active_domain_id_epid",
                schema: "ticketsystem",
                table: "purchase_order_master",
                columns: new[] { "is_active", "domain_id", "epid" });

            migrationBuilder.CreateIndex(
                name: "ix_asset_is_active_entity_type_entity_id",
                schema: "ticketsystem",
                table: "asset",
                columns: new[] { "is_active", "entity_type", "entity_id" });

            migrationBuilder.CreateIndex(
                name: "ix_ariba_tasks_is_active_approver_assigned_date",
                schema: "ticketsystem",
                table: "ariba_tasks",
                columns: new[] { "is_active", "approver", "assigned_date" });

            migrationBuilder.CreateIndex(
                name: "ix_ariba_prtasks_is_active_approver_assigned_date",
                schema: "ticketsystem",
                table: "ariba_prtasks",
                columns: new[] { "is_active", "approver", "assigned_date" })
                .Annotation("SqlServer:Include", new[] { "approval_id", "domain_id", "description", "email", "status", "approvable_type", "approvable_unique_name", "date_created", "date_updated", "created_by", "updated_by", "full_url", "payment_status" });

            migrationBuilder.CreateIndex(
                name: "ix_access_token_is_active_user_id",
                schema: "ticketsystem",
                table: "access_token",
                columns: new[] { "is_active", "user_id" })
                .Annotation("SqlServer:Include", new[] { "access_token_value", "expired_time", "date_created", "date_updated", "created_by", "updated_by", "refresh_token" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_ticket_is_active_domain_id",
                schema: "ticketsystem",
                table: "ticket");

            migrationBuilder.DropIndex(
                name: "ix_purchase_order_master_is_active_domain_id_epid",
                schema: "ticketsystem",
                table: "purchase_order_master");

            migrationBuilder.DropIndex(
                name: "ix_asset_is_active_entity_type_entity_id",
                schema: "ticketsystem",
                table: "asset");

            migrationBuilder.DropIndex(
                name: "ix_ariba_tasks_is_active_approver_assigned_date",
                schema: "ticketsystem",
                table: "ariba_tasks");

            migrationBuilder.DropIndex(
                name: "ix_ariba_prtasks_is_active_approver_assigned_date",
                schema: "ticketsystem",
                table: "ariba_prtasks");

            migrationBuilder.DropIndex(
                name: "ix_access_token_is_active_user_id",
                schema: "ticketsystem",
                table: "access_token");

            migrationBuilder.CreateIndex(
                name: "ix_ticket_is_active",
                schema: "ticketsystem",
                table: "ticket",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_purchase_order_master_is_active",
                schema: "ticketsystem",
                table: "purchase_order_master",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_asset_is_active",
                schema: "ticketsystem",
                table: "asset",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_ariba_tasks_is_active",
                schema: "ticketsystem",
                table: "ariba_tasks",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_ariba_prtasks_is_active",
                schema: "ticketsystem",
                table: "ariba_prtasks",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_access_token_is_active",
                schema: "ticketsystem",
                table: "access_token",
                column: "is_active");
        }
    }
}
