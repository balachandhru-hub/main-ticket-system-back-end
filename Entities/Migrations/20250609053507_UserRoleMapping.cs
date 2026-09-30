using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class UserRoleMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "user_role_mapping",
                schema: "ticketsystem",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    role_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    date_created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    date_updated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_role_mapping", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_role_mapping_user_user_id",
                        column: x => x.user_id,
                        principalSchema: "ticketsystem",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_user_is_active",
                schema: "ticketsystem",
                table: "user",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_ticket_is_active",
                schema: "ticketsystem",
                table: "ticket",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_role_feature_mapping_is_active",
                schema: "ticketsystem",
                table: "role_feature_mapping",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_role_is_active",
                schema: "ticketsystem",
                table: "role",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_feature_is_active",
                schema: "ticketsystem",
                table: "feature",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_asset_is_active",
                schema: "ticketsystem",
                table: "asset",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_access_token_is_active",
                schema: "ticketsystem",
                table: "access_token",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_user_role_mapping_is_active",
                schema: "ticketsystem",
                table: "user_role_mapping",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_user_role_mapping_user_id",
                schema: "ticketsystem",
                table: "user_role_mapping",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_role_mapping",
                schema: "ticketsystem");

            migrationBuilder.DropIndex(
                name: "ix_user_is_active",
                schema: "ticketsystem",
                table: "user");

            migrationBuilder.DropIndex(
                name: "ix_ticket_is_active",
                schema: "ticketsystem",
                table: "ticket");

            migrationBuilder.DropIndex(
                name: "ix_role_feature_mapping_is_active",
                schema: "ticketsystem",
                table: "role_feature_mapping");

            migrationBuilder.DropIndex(
                name: "ix_role_is_active",
                schema: "ticketsystem",
                table: "role");

            migrationBuilder.DropIndex(
                name: "ix_feature_is_active",
                schema: "ticketsystem",
                table: "feature");

            migrationBuilder.DropIndex(
                name: "ix_asset_is_active",
                schema: "ticketsystem",
                table: "asset");

            migrationBuilder.DropIndex(
                name: "ix_access_token_is_active",
                schema: "ticketsystem",
                table: "access_token");
        }
    }
}
