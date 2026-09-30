using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class ContractPending : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "contract_pending",
                schema: "ticketsystem",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    unique_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    domain_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    approver = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    assigned_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    full_url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    document_type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    date_created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    date_updated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_contract_pending", x => x.id);
                    table.ForeignKey(
                        name: "fk_contract_pending_domain_domain_id",
                        column: x => x.domain_id,
                        principalSchema: "ticketsystem",
                        principalTable: "domain",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "contract_attachment",
                schema: "ticketsystem",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    task_id = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    work_space_id = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    attachment_id = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    contract_pending_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    date_created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    date_updated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_contract_attachment", x => x.id);
                    table.ForeignKey(
                        name: "fk_contract_attachment_contract_pending_contract_pending_id",
                        column: x => x.contract_pending_id,
                        principalSchema: "ticketsystem",
                        principalTable: "contract_pending",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_contract_attachment_contract_pending_id",
                schema: "ticketsystem",
                table: "contract_attachment",
                column: "contract_pending_id");

            migrationBuilder.CreateIndex(
                name: "ix_contract_attachment_is_active",
                schema: "ticketsystem",
                table: "contract_attachment",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_contract_pending_domain_id",
                schema: "ticketsystem",
                table: "contract_pending",
                column: "domain_id");

            migrationBuilder.CreateIndex(
                name: "ix_contract_pending_is_active",
                schema: "ticketsystem",
                table: "contract_pending",
                column: "is_active");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "contract_attachment",
                schema: "ticketsystem");

            migrationBuilder.DropTable(
                name: "contract_pending",
                schema: "ticketsystem");
        }
    }
}
