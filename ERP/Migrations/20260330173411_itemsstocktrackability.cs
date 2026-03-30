using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Migrations
{
    /// <inheritdoc />
    public partial class itemsstocktrackability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SalesOrderItemId",
                table: "PurchaseOrderItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SalesOrderItemId",
                table: "ProductionOrders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderItems_SalesOrderItemId",
                table: "PurchaseOrderItems",
                column: "SalesOrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrders_SalesOrderItemId",
                table: "ProductionOrders",
                column: "SalesOrderItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionOrders_SalesOrderItems_SalesOrderItemId",
                table: "ProductionOrders",
                column: "SalesOrderItemId",
                principalTable: "SalesOrderItems",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrderItems_SalesOrderItems_SalesOrderItemId",
                table: "PurchaseOrderItems",
                column: "SalesOrderItemId",
                principalTable: "SalesOrderItems",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductionOrders_SalesOrderItems_SalesOrderItemId",
                table: "ProductionOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrderItems_SalesOrderItems_SalesOrderItemId",
                table: "PurchaseOrderItems");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrderItems_SalesOrderItemId",
                table: "PurchaseOrderItems");

            migrationBuilder.DropIndex(
                name: "IX_ProductionOrders_SalesOrderItemId",
                table: "ProductionOrders");

            migrationBuilder.DropColumn(
                name: "SalesOrderItemId",
                table: "PurchaseOrderItems");

            migrationBuilder.DropColumn(
                name: "SalesOrderItemId",
                table: "ProductionOrders");
        }
    }
}
