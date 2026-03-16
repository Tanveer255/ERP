using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedProductStocktable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductStock",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuantityAvailable = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0.0m),
                    QuantityReserved = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0.0m),
                    QuantityInProduction = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0.0m),
                    QuantityQuarantined = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0.0m),
                    QuantityRejected = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0.0m),
                    QuantityExpired = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0.0m),
                    Warehouse = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "None"),
                    Zone = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "None"),
                    Aisle = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "None"),
                    Rack = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "None"),
                    Shelf = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "None"),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductStock", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductStock_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductStock_ProductId",
                table: "ProductStock",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductStock");
        }
    }
}
