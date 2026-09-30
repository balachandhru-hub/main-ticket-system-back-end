using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedDomainPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "domain",
                schema: "ticketsystem",
                table: "user");

            migrationBuilder.DropColumn(
                name: "subscription_plan",
                schema: "ticketsystem",
                table: "user");

            migrationBuilder.AlterColumn<string>(
                name: "engagement_type",
                schema: "ticketsystem",
                table: "domain_plan",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<Guid>(
                name: "discount_coupon_id",
                schema: "ticketsystem",
                table: "domain_plan",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.CreateTable(
                name: "domain_realm",
                schema: "ticketsystem",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    domain_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    realm_id = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_domain_realm", x => x.id);
                    table.ForeignKey(
                        name: "fk_domain_realm_domain_domain_id",
                        column: x => x.domain_id,
                        principalSchema: "ticketsystem",
                        principalTable: "domain",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_domain_realm_domain_id",
                schema: "ticketsystem",
                table: "domain_realm",
                column: "domain_id");

            migrationBuilder.CreateIndex(
                name: "ix_domain_realm_is_active",
                schema: "ticketsystem",
                table: "domain_realm",
                column: "is_active");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "domain_realm",
                schema: "ticketsystem");

            migrationBuilder.AddColumn<string>(
                name: "domain",
                schema: "ticketsystem",
                table: "user",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "subscription_plan",
                schema: "ticketsystem",
                table: "user",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "engagement_type",
                schema: "ticketsystem",
                table: "domain_plan",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<Guid>(
                name: "discount_coupon_id",
                schema: "ticketsystem",
                table: "domain_plan",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }
    }
}
