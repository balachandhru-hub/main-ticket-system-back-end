using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class TicketNumberCreation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence<int>(
                name: "TicketCodeSequence",
                schema: "ticketsystem");

            migrationBuilder.AddColumn<string>(
                name: "ticket_code",
                schema: "ticketsystem",
                table: "ticket",
                type: "nvarchar(max)",
                nullable: true,
                defaultValueSql: "'TC' + RIGHT('000' + CAST(NEXT VALUE FOR [ticketsystem].[TicketCodeSequence] AS VARCHAR(3)), 3)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ticket_code",
                schema: "ticketsystem",
                table: "ticket");

            migrationBuilder.DropSequence(
                name: "TicketCodeSequence",
                schema: "ticketsystem");
        }
    }
}
