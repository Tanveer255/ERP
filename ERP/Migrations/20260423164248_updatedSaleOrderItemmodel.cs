using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Migrations
{
    /// <inheritdoc />
    public partial class updatedSaleOrderItemmodel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ProductionOrderId",
                table: "PurchaseOrderItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderItems_ProductionOrderId",
                table: "PurchaseOrderItems",
                column: "ProductionOrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrderItems_ProductionOrders_ProductionOrderId",
                table: "PurchaseOrderItems",
                column: "ProductionOrderId",
                principalTable: "ProductionOrders",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrderItems_ProductionOrders_ProductionOrderId",
                table: "PurchaseOrderItems");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrderItems_ProductionOrderId",
                table: "PurchaseOrderItems");

            migrationBuilder.DropColumn(
                name: "ProductionOrderId",
                table: "PurchaseOrderItems");
        }
    }
}
