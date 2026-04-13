using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LudaFit.Infrastructure.SQLite.Migrations
{
    /// <inheritdoc />
    public partial class AddDiscountIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Discounts_EndDate",
                table: "Discounts",
                column: "EndDate");
        }
        
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Discounts_EndDate",
                table: "Discounts");
        }
    }
}
