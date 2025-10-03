using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampusCafeOrderingSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOrdersIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderDate_Status",
                table: "Orders",
                columns: new[] { "OrderDate", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_Status_OrderDate",
                table: "Orders",
                columns: new[] { "Status", "OrderDate" });

            // MenuItemName index temporarily removed due to potential data type issues
            // migrationBuilder.CreateIndex(
            //     name: "IX_OrderItems_MenuItemName",
            //     table: "OrderItems",
            //     column: "MenuItemName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_OrderDate_Status",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_Status_OrderDate",
                table: "Orders");

            // Corresponding drop index also commented out
            // migrationBuilder.DropIndex(
            //     name: "IX_OrderItems_MenuItemName",
            //     table: "OrderItems");
        }
    }
}
