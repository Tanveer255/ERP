using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Migrations
{
    /// <inheritdoc />
    public partial class inventorysettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InventorySettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AutoReserveOnReceive = table.Column<bool>(type: "bit", nullable: false),
                    AutoFulfillOnReceive = table.Column<bool>(type: "bit", nullable: false),
                    AutoRunMrpOnReceive = table.Column<bool>(type: "bit", nullable: false),
                    AllowPartialFulfillment = table.Column<bool>(type: "bit", nullable: false),
                    AllowOverFulfill = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventorySettings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventorySettings");
        }
    }
}
