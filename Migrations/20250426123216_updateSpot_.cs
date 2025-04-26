using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TourismWeb.Migrations
{
    /// <inheritdoc />
    public partial class updateSpot_ : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TouristSpots_Users_UserId",
                table: "TouristSpots");

            migrationBuilder.DropIndex(
                name: "IX_TouristSpots_UserId",
                table: "TouristSpots");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "TouristSpots");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "TouristSpots",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TouristSpots_UserId",
                table: "TouristSpots",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TouristSpots_Users_UserId",
                table: "TouristSpots",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId");
        }
    }
}
