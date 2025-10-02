using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampusCafeOrderingSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingUserPreferenceColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "OrderStatusNotifications",
                table: "UserPreferences",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "PreferredPaymentMethod",
                table: "UserPreferences",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "CampusCard");

            migrationBuilder.AddColumn<bool>(
                name: "PromotionNotifications",
                table: "UserPreferences",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderStatusNotifications",
                table: "UserPreferences");

            migrationBuilder.DropColumn(
                name: "PreferredPaymentMethod",
                table: "UserPreferences");

            migrationBuilder.DropColumn(
                name: "PromotionNotifications",
                table: "UserPreferences");
        }
    }
}
