using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Migrations
{
    /// <inheritdoc />
    public partial class quantityaddedinsaleorderitems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Quantity",
                table: "SalesOrderItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "SalesOrderItems");
        }
    }
}
