using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class AribaPRHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ariba_prhistory",
                schema: "ticketsystem",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    requisitions_id = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ariba_task_id = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    approval_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ariba_user = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    comments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    level = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    date_created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    date_updated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ariba_prhistory", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_ariba_prtasks_is_active",
                schema: "ticketsystem",
                table: "ariba_prtasks",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_ariba_prattachment_is_active",
                schema: "ticketsystem",
                table: "ariba_prattachment",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_ariba_prhistory_is_active",
                schema: "ticketsystem",
                table: "ariba_prhistory",
                column: "is_active");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ariba_prhistory",
                schema: "ticketsystem");

            migrationBuilder.DropIndex(
                name: "ix_ariba_prtasks_is_active",
                schema: "ticketsystem",
                table: "ariba_prtasks");

            migrationBuilder.DropIndex(
                name: "ix_ariba_prattachment_is_active",
                schema: "ticketsystem",
                table: "ariba_prattachment");
        }
    }
}
