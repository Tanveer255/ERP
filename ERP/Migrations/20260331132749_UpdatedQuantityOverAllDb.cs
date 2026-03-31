using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedQuantityOverAllDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ReservedQuantity",
                table: "SalesOrderItems",
                newName: "QuantityReserved");

            migrationBuilder.RenameColumn(
                name: "RequestedQuantity",
                table: "SalesOrderItems",
                newName: "QuantityRequested");

            migrationBuilder.RenameColumn(
                name: "RequestedQuantity",
                table: "PurchaseOrderItems",
                newName: "QuantityRequested");

            migrationBuilder.RenameColumn(
                name: "ReceivedQuantity",
                table: "GoodsReceipts",
                newName: "QuantityReceived");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "QuantityReserved",
                table: "SalesOrderItems",
                newName: "ReservedQuantity");

            migrationBuilder.RenameColumn(
                name: "QuantityRequested",
                table: "SalesOrderItems",
                newName: "RequestedQuantity");

            migrationBuilder.RenameColumn(
                name: "QuantityRequested",
                table: "PurchaseOrderItems",
                newName: "RequestedQuantity");

            migrationBuilder.RenameColumn(
                name: "QuantityReceived",
                table: "GoodsReceipts",
                newName: "ReceivedQuantity");
        }
    }
}
