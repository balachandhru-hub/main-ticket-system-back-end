using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class username : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_login_record",
                schema: "ticketsystem",
                table: "login_record");

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                schema: "ticketsystem",
                table: "login_record",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
            
            migrationBuilder.AddPrimaryKey(
                name: "pk_login_record",
                schema: "ticketsystem",
                table: "login_record",
                column: "id");

            migrationBuilder.AddColumn<string>(
                name: "user_name",
                schema: "ticketsystem",
                table: "login_record",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            
            migrationBuilder.DropPrimaryKey(
                name: "pk_login_record",
                schema: "ticketsystem",
                table: "login_record");

            migrationBuilder.DropColumn(
                name: "user_name",
                schema: "ticketsystem",
                table: "login_record");

            migrationBuilder.AlterColumn<string>(
                name: "id",
                schema: "ticketsystem",
                table: "login_record",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddPrimaryKey(
                name: "pk_login_record",
                schema: "ticketsystem",
                table: "login_record",
                column: "id");
        }
    }
}
