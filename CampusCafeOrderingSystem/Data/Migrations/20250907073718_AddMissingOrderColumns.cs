using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampusCafeOrderingSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingOrderColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add missing columns to Orders table if they don't exist
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders' AND COLUMN_NAME = 'CreatedAt')
                BEGIN
                    ALTER TABLE Orders ADD CreatedAt datetime2 NOT NULL DEFAULT GETDATE()
                END
                
                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders' AND COLUMN_NAME = 'UpdatedAt')
                BEGIN
                    ALTER TABLE Orders ADD UpdatedAt datetime2 NOT NULL DEFAULT GETDATE()
                END
                
                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders' AND COLUMN_NAME = 'CustomerPhone')
                BEGIN
                    ALTER TABLE Orders ADD CustomerPhone nvarchar(20) NULL
                END
                
                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'OrderItems' AND COLUMN_NAME = 'SpecialInstructions')
                BEGIN
                    ALTER TABLE OrderItems ADD SpecialInstructions nvarchar(200) NULL
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove the columns if they exist
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders' AND COLUMN_NAME = 'CreatedAt')
                BEGIN
                    ALTER TABLE Orders DROP COLUMN CreatedAt
                END
                
                IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders' AND COLUMN_NAME = 'UpdatedAt')
                BEGIN
                    ALTER TABLE Orders DROP COLUMN UpdatedAt
                END
                
                IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders' AND COLUMN_NAME = 'CustomerPhone')
                BEGIN
                    ALTER TABLE Orders DROP COLUMN CustomerPhone
                END
                
                IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'OrderItems' AND COLUMN_NAME = 'SpecialInstructions')
                BEGIN
                    ALTER TABLE OrderItems DROP COLUMN SpecialInstructions
                END
            ");
        }
    }
}
