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

            // MenuItemName索引暂时移除，因为可能存在数据类型问题
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

            // 对应的删除索引也注释掉
            // migrationBuilder.DropIndex(
            //     name: "IX_OrderItems_MenuItemName",
            //     table: "OrderItems");
        }
    }
}
