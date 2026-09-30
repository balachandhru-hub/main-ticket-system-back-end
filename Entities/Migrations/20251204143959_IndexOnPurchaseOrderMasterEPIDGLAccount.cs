using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class IndexOnPurchaseOrderMasterEPIDGLAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_purchase_order_master_is_active_glaccount_epid",
                schema: "ticketsystem",
                table: "purchase_order_master",
                columns: new[] { "is_active", "glaccount", "epid" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_purchase_order_master_is_active_glaccount_epid",
                schema: "ticketsystem",
                table: "purchase_order_master");
        }
    }
}
