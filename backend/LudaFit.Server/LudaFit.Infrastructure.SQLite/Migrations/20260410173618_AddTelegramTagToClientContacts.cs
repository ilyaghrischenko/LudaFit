using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LudaFit.Infrastructure.SQLite.Migrations
{
    /// <inheritdoc />
    public partial class AddTelegramTagToClientContacts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClientTelegramTag",
                table: "Bookings",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientTelegramTag",
                table: "Bookings");
        }
    }
}
