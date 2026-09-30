using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedDomainTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "gst_vat",
                schema: "ticketsystem",
                table: "domain",
                newName: "ariba_network_id");

            migrationBuilder.AddColumn<string>(
                name: "description",
                schema: "ticketsystem",
                table: "metadata",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "countries_of_operation",
                schema: "ticketsystem",
                table: "domain",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "gst",
                schema: "ticketsystem",
                table: "domain",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "nda",
                schema: "ticketsystem",
                table: "domain",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "other_local_tax_id",
                schema: "ticketsystem",
                table: "domain",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "pan",
                schema: "ticketsystem",
                table: "domain",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tan",
                schema: "ticketsystem",
                table: "domain",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "taxregistration",
                schema: "ticketsystem",
                table: "domain",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "vat",
                schema: "ticketsystem",
                table: "domain",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "description",
                schema: "ticketsystem",
                table: "metadata");

            migrationBuilder.DropColumn(
                name: "gst",
                schema: "ticketsystem",
                table: "domain");

            migrationBuilder.DropColumn(
                name: "nda",
                schema: "ticketsystem",
                table: "domain");

            migrationBuilder.DropColumn(
                name: "other_local_tax_id",
                schema: "ticketsystem",
                table: "domain");

            migrationBuilder.DropColumn(
                name: "pan",
                schema: "ticketsystem",
                table: "domain");

            migrationBuilder.DropColumn(
                name: "tan",
                schema: "ticketsystem",
                table: "domain");

            migrationBuilder.DropColumn(
                name: "taxregistration",
                schema: "ticketsystem",
                table: "domain");

            migrationBuilder.DropColumn(
                name: "vat",
                schema: "ticketsystem",
                table: "domain");

            migrationBuilder.RenameColumn(
                name: "ariba_network_id",
                schema: "ticketsystem",
                table: "domain",
                newName: "gst_vat");

            migrationBuilder.AlterColumn<string>(
                name: "countries_of_operation",
                schema: "ticketsystem",
                table: "domain",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
