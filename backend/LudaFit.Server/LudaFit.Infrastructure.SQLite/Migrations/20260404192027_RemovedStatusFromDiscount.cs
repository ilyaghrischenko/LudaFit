using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LudaFit.Infrastructure.SQLite.Migrations
{
    /// <inheritdoc />
    public partial class RemovedStatusFromDiscount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Discounts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Discounts",
                type: "TEXT",
                nullable: false,
                defaultValue: string.Empty);
        }
    }
}
