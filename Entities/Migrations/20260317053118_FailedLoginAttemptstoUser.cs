using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class FailedLoginAttemptstoUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "failed_login_attempts",
                schema: "ticketsystem",
                table: "user",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "last_failed_login",
                schema: "ticketsystem",
                table: "user",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "lockout_end",
                schema: "ticketsystem",
                table: "user",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "failed_login_attempts",
                schema: "ticketsystem",
                table: "user");

            migrationBuilder.DropColumn(
                name: "last_failed_login",
                schema: "ticketsystem",
                table: "user");

            migrationBuilder.DropColumn(
                name: "lockout_end",
                schema: "ticketsystem",
                table: "user");
        }
    }
}
