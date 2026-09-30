using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class AribaPRs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_ariba_tasks_ariba_pr_ariba_prid",
                schema: "ticketsystem",
                table: "ariba_tasks");

            migrationBuilder.DropIndex(
                name: "ix_ariba_tasks_ariba_prid",
                schema: "ticketsystem",
                table: "ariba_tasks");

            migrationBuilder.DropColumn(
                name: "ariba_prid",
                schema: "ticketsystem",
                table: "ariba_tasks");

            migrationBuilder.CreateTable(
                name: "ariba_prtasks",
                schema: "ticketsystem",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    approval_id = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    domain_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    approver = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    assigned_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    full_url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    due_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    approvable_type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    approvable_unique_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ariba_prid = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    date_created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    date_updated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ariba_prtasks", x => x.id);
                    table.ForeignKey(
                        name: "fk_ariba_prtasks_ariba_pr_ariba_prid",
                        column: x => x.ariba_prid,
                        principalSchema: "ticketsystem",
                        principalTable: "ariba_pr",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_ariba_prtasks_domain_domain_id",
                        column: x => x.domain_id,
                        principalSchema: "ticketsystem",
                        principalTable: "domain",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ariba_prattachment",
                schema: "ticketsystem",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    requisitions_id = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ariba_task_id = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    attachment_id = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ariba_prtasks_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    date_created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    date_updated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ariba_prattachment", x => x.id);
                    table.ForeignKey(
                        name: "fk_ariba_prattachment_ariba_prtasks_ariba_prtasks_id",
                        column: x => x.ariba_prtasks_id,
                        principalSchema: "ticketsystem",
                        principalTable: "ariba_prtasks",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_ariba_prattachment_ariba_prtasks_id",
                schema: "ticketsystem",
                table: "ariba_prattachment",
                column: "ariba_prtasks_id");

            migrationBuilder.CreateIndex(
                name: "ix_ariba_prtasks_ariba_prid",
                schema: "ticketsystem",
                table: "ariba_prtasks",
                column: "ariba_prid");

            migrationBuilder.CreateIndex(
                name: "ix_ariba_prtasks_domain_id",
                schema: "ticketsystem",
                table: "ariba_prtasks",
                column: "domain_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ariba_prattachment",
                schema: "ticketsystem");

            migrationBuilder.DropTable(
                name: "ariba_prtasks",
                schema: "ticketsystem");

            migrationBuilder.AddColumn<Guid>(
                name: "ariba_prid",
                schema: "ticketsystem",
                table: "ariba_tasks",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_ariba_tasks_ariba_prid",
                schema: "ticketsystem",
                table: "ariba_tasks",
                column: "ariba_prid");

            migrationBuilder.AddForeignKey(
                name: "fk_ariba_tasks_ariba_pr_ariba_prid",
                schema: "ticketsystem",
                table: "ariba_tasks",
                column: "ariba_prid",
                principalSchema: "ticketsystem",
                principalTable: "ariba_pr",
                principalColumn: "id");
        }
    }
}
