using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class IndexesAttachmentTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ariba_id",
                schema: "ticketsystem",
                table: "final_prattachment",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ariba_id",
                schema: "ticketsystem",
                table: "final_attachment",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "ix_final_prattachment_ariba_id_is_active",
                schema: "ticketsystem",
                table: "final_prattachment",
                columns: new[] { "ariba_id", "is_active" });

            migrationBuilder.CreateIndex(
                name: "ix_final_attachment_ariba_id_is_active",
                schema: "ticketsystem",
                table: "final_attachment",
                columns: new[] { "ariba_id", "is_active" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_final_prattachment_ariba_id_is_active",
                schema: "ticketsystem",
                table: "final_prattachment");

            migrationBuilder.DropIndex(
                name: "ix_final_attachment_ariba_id_is_active",
                schema: "ticketsystem",
                table: "final_attachment");

            migrationBuilder.AlterColumn<string>(
                name: "ariba_id",
                schema: "ticketsystem",
                table: "final_prattachment",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "ariba_id",
                schema: "ticketsystem",
                table: "final_attachment",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
