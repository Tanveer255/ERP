using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Migrations
{
    /// <inheritdoc />
    public partial class RequestedQuantityinsaleorderitems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Quantity",
                table: "SalesOrderItems",
                newName: "RequestedQuantity");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RequestedQuantity",
                table: "SalesOrderItems",
                newName: "Quantity");
        }
    }
}
