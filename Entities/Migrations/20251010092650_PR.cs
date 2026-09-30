using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class PR : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "prtax_aomunt",
                schema: "ticketsystem",
                table: "ariba_pr",
                newName: "prtax_amount");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "prtax_amount",
                schema: "ticketsystem",
                table: "ariba_pr",
                newName: "prtax_aomunt");
        }
    }
}
