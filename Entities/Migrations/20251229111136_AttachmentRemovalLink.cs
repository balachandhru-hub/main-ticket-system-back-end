using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class AttachmentRemovalLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_contract_attachment_contract_pending_contract_pending_id",
                schema: "ticketsystem",
                table: "contract_attachment");

            migrationBuilder.DropIndex(
                name: "ix_contract_attachment_contract_pending_id",
                schema: "ticketsystem",
                table: "contract_attachment");

            migrationBuilder.DropColumn(
                name: "contract_pending_id",
                schema: "ticketsystem",
                table: "contract_attachment");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "contract_pending_id",
                schema: "ticketsystem",
                table: "contract_attachment",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_contract_attachment_contract_pending_id",
                schema: "ticketsystem",
                table: "contract_attachment",
                column: "contract_pending_id");

            migrationBuilder.AddForeignKey(
                name: "fk_contract_attachment_contract_pending_contract_pending_id",
                schema: "ticketsystem",
                table: "contract_attachment",
                column: "contract_pending_id",
                principalSchema: "ticketsystem",
                principalTable: "contract_pending",
                principalColumn: "id");
        }
    }
}
