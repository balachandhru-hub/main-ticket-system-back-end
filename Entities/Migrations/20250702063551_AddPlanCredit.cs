using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class AddPlanCredit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "domain_plan_credit",
                schema: "ticketsystem",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    domain_plan_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    monthly_hour_credit = table.Column<double>(type: "float", nullable: false),
                    total_hour_credit = table.Column<double>(type: "float", nullable: false),
                    date_created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    date_updated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_domain_plan_credit", x => x.id);
                    table.ForeignKey(
                        name: "fk_domain_plan_credit_domain_plan_domain_plan_id",
                        column: x => x.domain_plan_id,
                        principalSchema: "ticketsystem",
                        principalTable: "domain_plan",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_domain_plan_credit_domain_plan_id",
                schema: "ticketsystem",
                table: "domain_plan_credit",
                column: "domain_plan_id");

            migrationBuilder.CreateIndex(
                name: "ix_domain_plan_credit_is_active",
                schema: "ticketsystem",
                table: "domain_plan_credit",
                column: "is_active");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "domain_plan_credit",
                schema: "ticketsystem");

            migrationBuilder.DropTable(
                name: "domain_realm",
                schema: "ticketsystem");

            migrationBuilder.AddColumn<string>(
                name: "domain",
                schema: "ticketsystem",
                table: "user",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "subscription_plan",
                schema: "ticketsystem",
                table: "user",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
