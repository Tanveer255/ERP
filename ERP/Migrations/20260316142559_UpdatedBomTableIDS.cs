using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedBomTableIDS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductionOrders_BillOfMaterials_BOMId",
                table: "ProductionOrders");

            migrationBuilder.RenameColumn(
                name: "BOMId",
                table: "ProductionOrders",
                newName: "BillOfMaterialId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductionOrders_BOMId",
                table: "ProductionOrders",
                newName: "IX_ProductionOrders_BillOfMaterialId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionOrders_BillOfMaterials_BillOfMaterialId",
                table: "ProductionOrders",
                column: "BillOfMaterialId",
                principalTable: "BillOfMaterials",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductionOrders_BillOfMaterials_BillOfMaterialId",
                table: "ProductionOrders");

            migrationBuilder.RenameColumn(
                name: "BillOfMaterialId",
                table: "ProductionOrders",
                newName: "BOMId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductionOrders_BillOfMaterialId",
                table: "ProductionOrders",
                newName: "IX_ProductionOrders_BOMId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionOrders_BillOfMaterials_BOMId",
                table: "ProductionOrders",
                column: "BOMId",
                principalTable: "BillOfMaterials",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
