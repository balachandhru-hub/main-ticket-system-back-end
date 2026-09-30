using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class AddEasyContractRegionandall : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "commodity",
                schema: "ticketsystem",
                table: "easy_contract");

            migrationBuilder.DropColumn(
                name: "department",
                schema: "ticketsystem",
                table: "easy_contract");

            migrationBuilder.DropColumn(
                name: "region",
                schema: "ticketsystem",
                table: "easy_contract");

            migrationBuilder.CreateTable(
                name: "easy_contract_comoditity",
                schema: "ticketsystem",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    domain_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    contract_id = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    unique_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    contract_domain = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    date_created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    date_updated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_easy_contract_comoditity", x => x.id);
                    table.ForeignKey(
                        name: "fk_easy_contract_comoditity_domain_domain_id",
                        column: x => x.domain_id,
                        principalSchema: "ticketsystem",
                        principalTable: "domain",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "easy_contract_department",
                schema: "ticketsystem",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    domain_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    contract_id = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    unique_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    date_created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    date_updated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_easy_contract_department", x => x.id);
                    table.ForeignKey(
                        name: "fk_easy_contract_department_domain_domain_id",
                        column: x => x.domain_id,
                        principalSchema: "ticketsystem",
                        principalTable: "domain",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "easy_contract_region",
                schema: "ticketsystem",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    domain_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    contract_id = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    unique_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    date_created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    date_updated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_easy_contract_region", x => x.id);
                    table.ForeignKey(
                        name: "fk_easy_contract_region_domain_domain_id",
                        column: x => x.domain_id,
                        principalSchema: "ticketsystem",
                        principalTable: "domain",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_easy_contract_comoditity_domain_id",
                schema: "ticketsystem",
                table: "easy_contract_comoditity",
                column: "domain_id");

            migrationBuilder.CreateIndex(
                name: "ix_easy_contract_comoditity_is_active",
                schema: "ticketsystem",
                table: "easy_contract_comoditity",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_easy_contract_department_domain_id",
                schema: "ticketsystem",
                table: "easy_contract_department",
                column: "domain_id");

            migrationBuilder.CreateIndex(
                name: "ix_easy_contract_department_is_active",
                schema: "ticketsystem",
                table: "easy_contract_department",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_easy_contract_region_domain_id",
                schema: "ticketsystem",
                table: "easy_contract_region",
                column: "domain_id");

            migrationBuilder.CreateIndex(
                name: "ix_easy_contract_region_is_active",
                schema: "ticketsystem",
                table: "easy_contract_region",
                column: "is_active");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "easy_contract_comoditity",
                schema: "ticketsystem");

            migrationBuilder.DropTable(
                name: "easy_contract_department",
                schema: "ticketsystem");

            migrationBuilder.DropTable(
                name: "easy_contract_region",
                schema: "ticketsystem");

            migrationBuilder.AddColumn<string>(
                name: "commodity",
                schema: "ticketsystem",
                table: "easy_contract",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "department",
                schema: "ticketsystem",
                table: "easy_contract",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "region",
                schema: "ticketsystem",
                table: "easy_contract",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
