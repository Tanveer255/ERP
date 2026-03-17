using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Migrations
{
    /// <inheritdoc />
    public partial class updatedBillOfMaterialAndItemAndProductionOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BillOfMaterialItems_BillOfMaterials_BOMId",
                table: "BillOfMaterialItems");

            migrationBuilder.DropForeignKey(
                name: "FK_BillOfMaterials_Products_ComponentId",
                table: "BillOfMaterials");

            migrationBuilder.DropIndex(
                name: "IX_BillOfMaterials_ComponentId",
                table: "BillOfMaterials");

            migrationBuilder.DropColumn(
                name: "ComponentId",
                table: "BillOfMaterials");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "BillOfMaterials");

            migrationBuilder.RenameColumn(
                name: "BOMId",
                table: "BillOfMaterialItems",
                newName: "BillOfMaterialId");

            migrationBuilder.RenameIndex(
                name: "IX_BillOfMaterialItems_BOMId",
                table: "BillOfMaterialItems",
                newName: "IX_BillOfMaterialItems_BillOfMaterialId");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "ProductionOrders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<decimal>(
                name: "ProducedQuantity",
                table: "ProductionOrders",
                type: "decimal(18,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "PlannedQuantity",
                table: "ProductionOrders",
                type: "decimal(18,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "OrderNumber",
                table: "ProductionOrders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Unit",
                table: "BillOfMaterialItems",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "BillOfMaterialItems",
                type: "decimal(18,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.CreateTable(
                name: "StockTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PerformedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockTransactions", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_BillOfMaterialItems_BillOfMaterials_BillOfMaterialId",
                table: "BillOfMaterialItems",
                column: "BillOfMaterialId",
                principalTable: "BillOfMaterials",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BillOfMaterialItems_BillOfMaterials_BillOfMaterialId",
                table: "BillOfMaterialItems");

            migrationBuilder.DropTable(
                name: "StockTransactions");

            migrationBuilder.RenameColumn(
                name: "BillOfMaterialId",
                table: "BillOfMaterialItems",
                newName: "BOMId");

            migrationBuilder.RenameIndex(
                name: "IX_BillOfMaterialItems_BillOfMaterialId",
                table: "BillOfMaterialItems",
                newName: "IX_BillOfMaterialItems_BOMId");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "ProductionOrders",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<decimal>(
                name: "ProducedQuantity",
                table: "ProductionOrders",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)");

            migrationBuilder.AlterColumn<decimal>(
                name: "PlannedQuantity",
                table: "ProductionOrders",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)");

            migrationBuilder.AlterColumn<string>(
                name: "OrderNumber",
                table: "ProductionOrders",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<Guid>(
                name: "ComponentId",
                table: "BillOfMaterials",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<decimal>(
                name: "Quantity",
                table: "BillOfMaterials",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "Unit",
                table: "BillOfMaterialItems",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "BillOfMaterialItems",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)");

            migrationBuilder.CreateIndex(
                name: "IX_BillOfMaterials_ComponentId",
                table: "BillOfMaterials",
                column: "ComponentId");

            migrationBuilder.AddForeignKey(
                name: "FK_BillOfMaterialItems_BillOfMaterials_BOMId",
                table: "BillOfMaterialItems",
                column: "BOMId",
                principalTable: "BillOfMaterials",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BillOfMaterials_Products_ComponentId",
                table: "BillOfMaterials",
                column: "ComponentId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
