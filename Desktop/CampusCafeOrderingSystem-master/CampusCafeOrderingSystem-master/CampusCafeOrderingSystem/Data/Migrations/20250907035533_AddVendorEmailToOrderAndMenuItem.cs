using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampusCafeOrderingSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddVendorEmailToOrderAndMenuItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VendorEmail",
                table: "Orders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VendorEmail",
                table: "MenuItems",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VendorEmail",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "VendorEmail",
                table: "MenuItems");
        }
    }
}
