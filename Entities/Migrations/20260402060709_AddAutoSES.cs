using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class AddAutoSES : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "auto_grn",
                schema: "ticketsystem",
                table: "draft_service_pr",
                newName: "auto_ses");

            migrationBuilder.CreateIndex(
                name: "ix_draft_service_pr_attachment_is_active",
                schema: "ticketsystem",
                table: "draft_service_pr_attachment",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_draft_service_pr_is_active",
                schema: "ticketsystem",
                table: "draft_service_pr",
                column: "is_active");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_draft_service_pr_attachment_is_active",
                schema: "ticketsystem",
                table: "draft_service_pr_attachment");

            migrationBuilder.DropIndex(
                name: "ix_draft_service_pr_is_active",
                schema: "ticketsystem",
                table: "draft_service_pr");

            migrationBuilder.RenameColumn(
                name: "auto_ses",
                schema: "ticketsystem",
                table: "draft_service_pr",
                newName: "auto_grn");
        }
    }
}
