using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketAndTicketPRAndUserPlanCredit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "total_credit",
                schema: "ticketsystem",
                table: "user_plan_credit");

            migrationBuilder.AddColumn<double>(
                name: "monthly_hours_credit",
                schema: "ticketsystem",
                table: "user_plan_credit",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "number_of_consults",
                schema: "ticketsystem",
                table: "user_plan_credit",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "plan_monthly_hours_credit",
                schema: "ticketsystem",
                table: "user_plan_credit",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "total_hours",
                schema: "ticketsystem",
                table: "user_plan_credit",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "priority",
                schema: "ticketsystem",
                table: "ticket",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "total_hours",
                schema: "ticketsystem",
                table: "ticket",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "type",
                schema: "ticketsystem",
                table: "ticket",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "type_category",
                schema: "ticketsystem",
                table: "ticket",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ticket_pr",
                schema: "ticketsystem",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ticket_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    task_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    hrs = table.Column<double>(type: "float", nullable: false),
                    amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    date_created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    date_updated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ticket_pr", x => x.id);
                    table.ForeignKey(
                        name: "fk_ticket_pr_ticket_ticket_id",
                        column: x => x.ticket_id,
                        principalSchema: "ticketsystem",
                        principalTable: "ticket",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_ticket_pr_is_active",
                schema: "ticketsystem",
                table: "ticket_pr",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_ticket_pr_ticket_id",
                schema: "ticketsystem",
                table: "ticket_pr",
                column: "ticket_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ticket_pr",
                schema: "ticketsystem");

            migrationBuilder.DropColumn(
                name: "monthly_hours_credit",
                schema: "ticketsystem",
                table: "user_plan_credit");

            migrationBuilder.DropColumn(
                name: "number_of_consults",
                schema: "ticketsystem",
                table: "user_plan_credit");

            migrationBuilder.DropColumn(
                name: "plan_monthly_hours_credit",
                schema: "ticketsystem",
                table: "user_plan_credit");

            migrationBuilder.DropColumn(
                name: "total_hours",
                schema: "ticketsystem",
                table: "user_plan_credit");

            migrationBuilder.DropColumn(
                name: "priority",
                schema: "ticketsystem",
                table: "ticket");

            migrationBuilder.DropColumn(
                name: "total_hours",
                schema: "ticketsystem",
                table: "ticket");

            migrationBuilder.DropColumn(
                name: "type",
                schema: "ticketsystem",
                table: "ticket");

            migrationBuilder.DropColumn(
                name: "type_category",
                schema: "ticketsystem",
                table: "ticket");

            migrationBuilder.AddColumn<int>(
                name: "total_credit",
                schema: "ticketsystem",
                table: "user_plan_credit",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
