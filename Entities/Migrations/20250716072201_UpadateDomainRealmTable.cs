using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class UpadateDomainRealmTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "created_by",
                schema: "ticketsystem",
                table: "domain_realm",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "date_created",
                schema: "ticketsystem",
                table: "domain_realm",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "date_updated",
                schema: "ticketsystem",
                table: "domain_realm",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "updated_by",
                schema: "ticketsystem",
                table: "domain_realm",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "created_by",
                schema: "ticketsystem",
                table: "domain_realm");

            migrationBuilder.DropColumn(
                name: "date_created",
                schema: "ticketsystem",
                table: "domain_realm");

            migrationBuilder.DropColumn(
                name: "date_updated",
                schema: "ticketsystem",
                table: "domain_realm");

            migrationBuilder.DropColumn(
                name: "updated_by",
                schema: "ticketsystem",
                table: "domain_realm");
        }
    }
}
