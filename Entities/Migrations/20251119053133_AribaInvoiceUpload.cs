using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class AribaInvoiceUpload : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ariba_invoice_upload",
                schema: "ticketsystem",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    invoice_number = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    approver = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    asset_id = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    date_created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    date_updated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ariba_invoice_upload", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ariba_prupload",
                schema: "ticketsystem",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    prnumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    approver = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    asset_id = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    date_created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    date_updated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ariba_prupload", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_user_model_name_is_active",
                schema: "ticketsystem",
                table: "user_model_name",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_ariba_invoice_upload_is_active",
                schema: "ticketsystem",
                table: "ariba_invoice_upload",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_ariba_prupload_is_active",
                schema: "ticketsystem",
                table: "ariba_prupload",
                column: "is_active");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ariba_invoice_upload",
                schema: "ticketsystem");

            migrationBuilder.DropTable(
                name: "ariba_prupload",
                schema: "ticketsystem");

            migrationBuilder.DropIndex(
                name: "ix_user_model_name_is_active",
                schema: "ticketsystem",
                table: "user_model_name");
        }
    }
}
