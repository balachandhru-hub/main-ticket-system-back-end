using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class AribaPR : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ariba_prid",
                schema: "ticketsystem",
                table: "ariba_tasks",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ariba_pr",
                schema: "ticketsystem",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    requisitions_id = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    domain_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    supplier_name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    currency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    due_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    requisitions_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    department = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ariba_task_id = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    contract_id = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    company_unique_name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    is_attachment_processed = table.Column<bool>(type: "bit", nullable: false),
                    date_created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    date_updated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ariba_pr", x => x.id);
                    table.ForeignKey(
                        name: "fk_ariba_pr_domain_domain_id",
                        column: x => x.domain_id,
                        principalSchema: "ticketsystem",
                        principalTable: "domain",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_ariba_tasks_ariba_prid",
                schema: "ticketsystem",
                table: "ariba_tasks",
                column: "ariba_prid");

            migrationBuilder.CreateIndex(
                name: "ix_ariba_pr_domain_id",
                schema: "ticketsystem",
                table: "ariba_pr",
                column: "domain_id");

            migrationBuilder.CreateIndex(
                name: "ix_ariba_pr_is_active",
                schema: "ticketsystem",
                table: "ariba_pr",
                column: "is_active");

            migrationBuilder.AddForeignKey(
                name: "fk_ariba_tasks_ariba_pr_ariba_prid",
                schema: "ticketsystem",
                table: "ariba_tasks",
                column: "ariba_prid",
                principalSchema: "ticketsystem",
                principalTable: "ariba_pr",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_ariba_tasks_ariba_pr_ariba_prid",
                schema: "ticketsystem",
                table: "ariba_tasks");

            migrationBuilder.DropTable(
                name: "ariba_pr",
                schema: "ticketsystem");

            migrationBuilder.DropIndex(
                name: "ix_ariba_tasks_ariba_prid",
                schema: "ticketsystem",
                table: "ariba_tasks");

            migrationBuilder.DropColumn(
                name: "ariba_prid",
                schema: "ticketsystem",
                table: "ariba_tasks");
        }
    }
}
