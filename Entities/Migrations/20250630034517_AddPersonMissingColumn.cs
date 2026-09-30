using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonMissingColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_plan_credit",
                schema: "ticketsystem");

            migrationBuilder.DropColumn(
                name: "type_category",
                schema: "ticketsystem",
                table: "ticket");

            migrationBuilder.RenameColumn(
                name: "total_ticket",
                schema: "ticketsystem",
                table: "user_ticket_count",
                newName: "total_hours");

            migrationBuilder.AlterColumn<float>(
                name: "total_hours",
                schema: "ticketsystem",
                table: "ticket",
                type: "real",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AddColumn<decimal>(
                name: "amount",
                schema: "ticketsystem",
                table: "ticket",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "domain_id",
                schema: "ticketsystem",
                table: "ticket",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "end_date",
                schema: "ticketsystem",
                table: "ticket",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "is_approved",
                schema: "ticketsystem",
                table: "ticket",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "plan",
                schema: "ticketsystem",
                table: "ticket",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "tenant_id",
                schema: "ticketsystem",
                table: "person",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "address",
                schema: "ticketsystem",
                table: "person",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "domain_id",
                schema: "ticketsystem",
                table: "person",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "mobile_phone",
                schema: "ticketsystem",
                table: "person",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "realm_id",
                schema: "ticketsystem",
                table: "person",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "discount_coupon",
                schema: "ticketsystem",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    coupon_code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    discount_percentage = table.Column<int>(type: "int", nullable: false),
                    date_created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    date_updated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_discount_coupon", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "domain",
                schema: "ticketsystem",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    website_url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    registration_number = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    headquarters_location = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    countries_of_operation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    business_type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ariba_realm_id = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    industry_type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    billing_address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    gst_vat = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    tax_applicable = table.Column<bool>(type: "bit", nullable: false),
                    number_of_user = table.Column<int>(type: "int", nullable: false),
                    support_admin_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    support_admin_email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    support_admin_time_zone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    support_admin_phone_no = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    connect_erpsystem = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    designation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    date_created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    date_updated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_domain", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ariba_usage_profile",
                schema: "ticketsystem",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    domain_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ariba_module = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    date_created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    date_updated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ariba_usage_profile", x => x.id);
                    table.ForeignKey(
                        name: "fk_ariba_usage_profile_domain_domain_id",
                        column: x => x.domain_id,
                        principalSchema: "ticketsystem",
                        principalTable: "domain",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "domain_plan",
                schema: "ticketsystem",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    domain_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    payment_status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    plan_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    engagement_type = table.Column<int>(type: "int", nullable: false),
                    start_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    end_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    discount_coupon_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    date_created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    date_updated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_domain_plan", x => x.id);
                    table.ForeignKey(
                        name: "fk_domain_plan_domain_domain_id",
                        column: x => x.domain_id,
                        principalSchema: "ticketsystem",
                        principalTable: "domain",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "plan_details",
                schema: "ticketsystem",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    domain_plan_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    crhours_beyond_limit = table.Column<int>(type: "int", nullable: false),
                    number_of_consult = table.Column<int>(type: "int", nullable: false),
                    number_of_manager = table.Column<int>(type: "int", nullable: false),
                    ticket_charge = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    monthly_hours = table.Column<int>(type: "int", nullable: false),
                    additional_crhours_charge = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    number_of_resources = table.Column<int>(type: "int", nullable: false),
                    crcost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    resource_cost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    manager_cost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    total_charge_of_consult = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    total_charge_of_manager = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    total_cost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    date_created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    date_updated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_plan_details", x => x.id);
                    table.ForeignKey(
                        name: "fk_plan_details_domain_plan_domain_plan_id",
                        column: x => x.domain_plan_id,
                        principalSchema: "ticketsystem",
                        principalTable: "domain_plan",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_person_domain_id",
                schema: "ticketsystem",
                table: "person",
                column: "domain_id");

            migrationBuilder.CreateIndex(
                name: "ix_ariba_usage_profile_domain_id",
                schema: "ticketsystem",
                table: "ariba_usage_profile",
                column: "domain_id");

            migrationBuilder.CreateIndex(
                name: "ix_ariba_usage_profile_is_active",
                schema: "ticketsystem",
                table: "ariba_usage_profile",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_discount_coupon_is_active",
                schema: "ticketsystem",
                table: "discount_coupon",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_domain_is_active",
                schema: "ticketsystem",
                table: "domain",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_domain_plan_domain_id",
                schema: "ticketsystem",
                table: "domain_plan",
                column: "domain_id");

            migrationBuilder.CreateIndex(
                name: "ix_domain_plan_is_active",
                schema: "ticketsystem",
                table: "domain_plan",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_plan_details_domain_plan_id",
                schema: "ticketsystem",
                table: "plan_details",
                column: "domain_plan_id");

            migrationBuilder.CreateIndex(
                name: "ix_plan_details_is_active",
                schema: "ticketsystem",
                table: "plan_details",
                column: "is_active");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ariba_usage_profile",
                schema: "ticketsystem");

            migrationBuilder.DropTable(
                name: "discount_coupon",
                schema: "ticketsystem");

            migrationBuilder.DropTable(
                name: "plan_details",
                schema: "ticketsystem");

            migrationBuilder.DropTable(
                name: "domain_plan",
                schema: "ticketsystem");

            migrationBuilder.DropTable(
                name: "domain",
                schema: "ticketsystem");

            migrationBuilder.DropIndex(
                name: "ix_person_domain_id",
                schema: "ticketsystem",
                table: "person");

            migrationBuilder.DropColumn(
                name: "amount",
                schema: "ticketsystem",
                table: "ticket");

            migrationBuilder.DropColumn(
                name: "domain_id",
                schema: "ticketsystem",
                table: "ticket");

            migrationBuilder.DropColumn(
                name: "end_date",
                schema: "ticketsystem",
                table: "ticket");

            migrationBuilder.DropColumn(
                name: "is_approved",
                schema: "ticketsystem",
                table: "ticket");

            migrationBuilder.DropColumn(
                name: "plan",
                schema: "ticketsystem",
                table: "ticket");

            migrationBuilder.DropColumn(
                name: "address",
                schema: "ticketsystem",
                table: "person");

            migrationBuilder.DropColumn(
                name: "domain_id",
                schema: "ticketsystem",
                table: "person");

            migrationBuilder.DropColumn(
                name: "mobile_phone",
                schema: "ticketsystem",
                table: "person");

            migrationBuilder.DropColumn(
                name: "realm_id",
                schema: "ticketsystem",
                table: "person");

            migrationBuilder.RenameColumn(
                name: "total_hours",
                schema: "ticketsystem",
                table: "user_ticket_count",
                newName: "total_ticket");

            migrationBuilder.AlterColumn<double>(
                name: "total_hours",
                schema: "ticketsystem",
                table: "ticket",
                type: "float",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AddColumn<string>(
                name: "type_category",
                schema: "ticketsystem",
                table: "ticket",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "tenant_id",
                schema: "ticketsystem",
                table: "person",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "user_plan_credit",
                schema: "ticketsystem",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    date_created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    date_updated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    domain = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    monthly_hours_credit = table.Column<double>(type: "float", nullable: false),
                    number_of_consults = table.Column<double>(type: "float", nullable: false),
                    plan_monthly_hours_credit = table.Column<double>(type: "float", nullable: false),
                    total_hours = table.Column<double>(type: "float", nullable: false),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_plan_credit", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_user_plan_credit_is_active",
                schema: "ticketsystem",
                table: "user_plan_credit",
                column: "is_active");
        }
    }
}
