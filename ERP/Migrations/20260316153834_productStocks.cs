using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Migrations
{
    /// <inheritdoc />
    public partial class productStocks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductStock_Products_ProductId",
                table: "ProductStock");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductStock",
                table: "ProductStock");

            migrationBuilder.RenameTable(
                name: "ProductStock",
                newName: "productStocks");

            migrationBuilder.RenameIndex(
                name: "IX_ProductStock_ProductId",
                table: "productStocks",
                newName: "IX_productStocks_ProductId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_productStocks",
                table: "productStocks",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_productStocks_Products_ProductId",
                table: "productStocks",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_productStocks_Products_ProductId",
                table: "productStocks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_productStocks",
                table: "productStocks");

            migrationBuilder.RenameTable(
                name: "productStocks",
                newName: "ProductStock");

            migrationBuilder.RenameIndex(
                name: "IX_productStocks_ProductId",
                table: "ProductStock",
                newName: "IX_ProductStock_ProductId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductStock",
                table: "ProductStock",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductStock_Products_ProductId",
                table: "ProductStock",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
