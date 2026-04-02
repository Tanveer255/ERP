using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Migrations
{
    /// <inheritdoc />
    public partial class updatedsaleOrderAndItemsModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PurchaseOrderId",
                table: "SalesOrders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalFulfilledQuantity",
                table: "SalesOrders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalQuantity",
                table: "SalesOrders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "QuantityFulfilled",
                table: "SalesOrderItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "SalesOrderItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PurchaseOrderId",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "TotalFulfilledQuantity",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "TotalQuantity",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "QuantityFulfilled",
                table: "SalesOrderItems");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "SalesOrderItems");
        }
    }
}
