using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedBomTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MaterialId",
                table: "BillOfMaterialItems",
                newName: "ComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_BillOfMaterialItems_ComponentId",
                table: "BillOfMaterialItems",
                column: "ComponentId");

            migrationBuilder.AddForeignKey(
                name: "FK_BillOfMaterialItems_Products_ComponentId",
                table: "BillOfMaterialItems",
                column: "ComponentId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BillOfMaterialItems_Products_ComponentId",
                table: "BillOfMaterialItems");

            migrationBuilder.DropIndex(
                name: "IX_BillOfMaterialItems_ComponentId",
                table: "BillOfMaterialItems");

            migrationBuilder.RenameColumn(
                name: "ComponentId",
                table: "BillOfMaterialItems",
                newName: "MaterialId");
        }
    }
}
