using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class AribaFinalAttachment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "invoice_id",
                schema: "ticketsystem",
                table: "final_attachment",
                newName: "type");

            // 1. Add a temporary column
            migrationBuilder.AddColumn<byte[]>(
                name: "attachment_tmp",
                schema: "ticketsystem",
                table: "final_attachment",
                type: "VARBINARY(MAX)",
                nullable: true);

            // 2. Copy data with explicit conversion
            migrationBuilder.Sql(@"
        UPDATE [ticketsystem].[final_attachment]
        SET [attachment_tmp] = CONVERT(VARBINARY(MAX), [attachment]);
    ");

            // 3. Drop old column
            migrationBuilder.DropColumn(
                name: "attachment",
                schema: "ticketsystem",
                table: "final_attachment");

            // 4. Rename temp column to real column
            migrationBuilder.RenameColumn(
                name: "attachment_tmp",
                schema: "ticketsystem",
                table: "final_attachment",
                newName: "attachment");

            // 5. Add your new column
            migrationBuilder.AddColumn<string>(
                name: "ariba_id",
                schema: "ticketsystem",
                table: "final_attachment",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ariba_id",
                schema: "ticketsystem",
                table: "final_attachment");

            migrationBuilder.RenameColumn(
                name: "type",
                schema: "ticketsystem",
                table: "final_attachment",
                newName: "invoice_id");

            // 1. Add a temporary column
            migrationBuilder.AddColumn<string>(
                name: "attachment_tmp",
                schema: "ticketsystem",
                table: "final_attachment",
                type: "nvarchar(max)",
                nullable: true);

            // 2. Convert back to NVARCHAR
            migrationBuilder.Sql(@"
        UPDATE [ticketsystem].[final_attachment]
        SET [attachment_tmp] = CONVERT(NVARCHAR(MAX), [attachment]);
    ");

            // 3. Drop old column
            migrationBuilder.DropColumn(
                name: "attachment",
                schema: "ticketsystem",
                table: "final_attachment");

            // 4. Rename temp column back
            migrationBuilder.RenameColumn(
                name: "attachment_tmp",
                schema: "ticketsystem",
                table: "final_attachment",
                newName: "attachment");
        }

    }
}
