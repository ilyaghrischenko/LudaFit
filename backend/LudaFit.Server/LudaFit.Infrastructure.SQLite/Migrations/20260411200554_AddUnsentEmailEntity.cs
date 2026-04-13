using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LudaFit.Infrastructure.SQLite.Migrations
{
    /// <inheritdoc />
    public partial class AddUnsentEmailEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UnsentEmails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AttemptNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    NextAttemptAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    BookingId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnsentEmails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnsentEmails_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UnsentEmails_AttemptNumber_NextAttemptAt",
                table: "UnsentEmails",
                columns: new[] { "AttemptNumber", "NextAttemptAt" });

            migrationBuilder.CreateIndex(
                name: "IX_UnsentEmails_BookingId",
                table: "UnsentEmails",
                column: "BookingId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UnsentEmails");
        }
    }
}
