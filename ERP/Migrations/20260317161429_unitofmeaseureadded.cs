using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Migrations
{
    /// <inheritdoc />
    public partial class unitofmeaseureadded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_productStocks_Products_ProductId",
                table: "productStocks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_productStocks",
                table: "productStocks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_prices",
                table: "prices");

            migrationBuilder.RenameTable(
                name: "productStocks",
                newName: "ProductStocks");

            migrationBuilder.RenameTable(
                name: "prices",
                newName: "Prices");

            migrationBuilder.RenameIndex(
                name: "IX_productStocks_ProductId",
                table: "ProductStocks",
                newName: "IX_ProductStocks_ProductId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductStocks",
                table: "ProductStocks",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Prices",
                table: "Prices",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "UnitOfMeasures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitOfMeasures", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_ProductStocks_Products_ProductId",
                table: "ProductStocks",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductStocks_Products_ProductId",
                table: "ProductStocks");

            migrationBuilder.DropTable(
                name: "UnitOfMeasures");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductStocks",
                table: "ProductStocks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Prices",
                table: "Prices");

            migrationBuilder.RenameTable(
                name: "ProductStocks",
                newName: "productStocks");

            migrationBuilder.RenameTable(
                name: "Prices",
                newName: "prices");

            migrationBuilder.RenameIndex(
                name: "IX_ProductStocks_ProductId",
                table: "productStocks",
                newName: "IX_productStocks_ProductId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_productStocks",
                table: "productStocks",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_prices",
                table: "prices",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_productStocks_Products_ProductId",
                table: "productStocks",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
