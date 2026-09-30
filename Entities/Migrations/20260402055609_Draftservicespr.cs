using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class Draftservicespr : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
    name: "draft_service_pr",
    schema: "ticketsystem",
    columns: table => new
    {
        id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),

        domain_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
        user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),

        purchase_org = table.Column<string>(type: "nvarchar(max)", nullable: true),
        description = table.Column<string>(type: "nvarchar(max)", nullable: true),
        draft_id = table.Column<string>(type: "nvarchar(max)", nullable: false),
        commodity_code = table.Column<string>(type: "nvarchar(max)", nullable: true),
        account_type = table.Column<string>(type: "nvarchar(max)", nullable: true),
        type = table.Column<string>(type: "nvarchar(max)", nullable: true),

        amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
        delivery_date = table.Column<DateTime>(type: "datetime2", nullable: true),
        quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: true),

        uom = table.Column<string>(type: "nvarchar(max)", nullable: true),
        vendor = table.Column<string>(type: "nvarchar(max)", nullable: true),
        contract = table.Column<string>(type: "nvarchar(max)", nullable: true),
        material_code = table.Column<string>(type: "nvarchar(max)", nullable: true),

        title = table.Column<string>(type: "nvarchar(max)", nullable: true),
        currency = table.Column<string>(type: "nvarchar(max)", nullable: true),
        requester_name = table.Column<string>(type: "nvarchar(max)", nullable: true),
        glaccount = table.Column<string>(type: "nvarchar(max)", nullable: true),
        cost_center = table.Column<string>(type: "nvarchar(max)", nullable: true),

        ship_to_plant = table.Column<string>(type: "nvarchar(max)", nullable: true),
        company_code = table.Column<string>(type: "nvarchar(max)", nullable: true),
        purchase_group = table.Column<string>(type: "nvarchar(max)", nullable: true),
        bill_to = table.Column<string>(type: "nvarchar(max)", nullable: true),
        tax_code = table.Column<string>(type: "nvarchar(max)", nullable: true),

        header_comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
        deliver_to = table.Column<string>(type: "nvarchar(max)", nullable: true),
        error_comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
        status = table.Column<string>(type: "nvarchar(max)", nullable: true),

        need_by = table.Column<DateTime>(type: "datetime2", nullable: true),
        auto_grn = table.Column<bool>(type: "bit", nullable: true),
        auto_invoices = table.Column<bool>(type: "bit", nullable: true),
        tax_include = table.Column<bool>(type: "bit", nullable: true),

        venue = table.Column<string>(type: "nvarchar(max)", nullable: true),
        ssid = table.Column<string>(type: "nvarchar(max)", nullable: true),
        invoice_id = table.Column<string>(type: "nvarchar(max)", nullable: true),
        invoice_date = table.Column<DateTime>(type: "datetime2", nullable: true),

        is_blocked = table.Column<bool>(type: "bit", nullable: true),
        processed_state = table.Column<int>(type: "int", nullable: true),
        line_item_number = table.Column<int>(type: "int", nullable: true),

        prservice_unique_name = table.Column<string>(type: "nvarchar(max)", nullable: true),
        counter = table.Column<string>(type: "nvarchar(max)", nullable: true),
        ponumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
        preparer_password_adapter = table.Column<string>(type: "nvarchar(max)", nullable: true),

        vendor_name = table.Column<string>(type: "nvarchar(max)", nullable: true),
        is_ocrattachment_processed = table.Column<bool>(type: "bit", nullable: true),
        invoice_comment = table.Column<string>(type: "nvarchar(max)", nullable: true),

        parent_line_item = table.Column<int>(type: "int", nullable: true),

        service_start_date = table.Column<DateTime>(type: "datetime2", nullable: true),
        service_end_date = table.Column<DateTime>(type: "datetime2", nullable: true),

        category = table.Column<string>(type: "nvarchar(max)", nullable: true),

        // BaseModel fields (VERY IMPORTANT)
        created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
        updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
        date_created = table.Column<DateTime>(type: "datetime2", nullable: false),
        date_updated = table.Column<DateTime>(type: "datetime2", nullable: false),
        is_active = table.Column<bool>(type: "bit", nullable: false)
    },
    constraints: table =>
    {
        table.PrimaryKey("pk_draft_service_pr", x => x.id);
        table.ForeignKey(
            name: "fk_draft_service_pr_domain_domain_id",
            column: x => x.domain_id,
            principalSchema: "ticketsystem",
            principalTable: "domain",       
            principalColumn: "id",
            onDelete: ReferentialAction.Cascade);
    });
            migrationBuilder.CreateTable(
            name: "draft_service_pr_attachment",
            schema: "ticketsystem",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),

                domain_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),

                draft_id = table.Column<string>(type: "nvarchar(max)", nullable: false),
                asset_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                invoice_id = table.Column<string>(type: "nvarchar(max)", nullable: true),
                line_item_number = table.Column<int>(type: "int", nullable: true),

                // BaseModel fields
                created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                date_created = table.Column<DateTime>(type: "datetime2", nullable: false),
                date_updated = table.Column<DateTime>(type: "datetime2", nullable: false),
                is_active = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_draft_service_pr_attachment", x => x.id);
                table.ForeignKey(
                    name: "fk_draft_service_pr_attachment_domain_domain_id",
                    column: x => x.domain_id,
                    principalSchema: "ticketsystem",
                    principalTable: "domain",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "draft_service_pr_attachment",
                schema: "ticketsystem");
 
            migrationBuilder.DropTable(
                name: "draft_service_pr",
                schema: "ticketsystem");
        }
    }
}
