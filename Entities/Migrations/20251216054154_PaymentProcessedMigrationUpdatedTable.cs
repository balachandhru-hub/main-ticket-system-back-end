using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class PaymentProcessedMigrationUpdatedTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ariba_payment",
                schema: "ticketsystem",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    domain_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    prnumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ponumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    invoice_number = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    invoice_amount = table.Column<double>(type: "float", nullable: true),
                    ariba_payment_id = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    erpinvoice_number = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    advance_payment_id = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    advance_amount = table.Column<double>(type: "float", nullable: true),
                    advance_payment_status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    payment_status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    payment_id = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    paid_amount = table.Column<double>(type: "float", nullable: true),
                    paid_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    approver = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    bank_clearance = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    bank_clearance_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    date_created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    date_updated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ariba_payment", x => x.id);
                    table.ForeignKey(
                        name: "fk_ariba_payment_domain_domain_id",
                        column: x => x.domain_id,
                        principalSchema: "ticketsystem",
                        principalTable: "domain",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_ariba_payment_domain_id",
                schema: "ticketsystem",
                table: "ariba_payment",
                column: "domain_id");

            migrationBuilder.CreateIndex(
                name: "ix_ariba_payment_is_active",
                schema: "ticketsystem",
                table: "ariba_payment",
                column: "is_active");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ariba_payment",
                schema: "ticketsystem");
        }
    }
}
