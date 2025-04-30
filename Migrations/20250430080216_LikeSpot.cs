using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TourismWeb.Migrations
{
    /// <inheritdoc />
    public partial class LikeSpot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsLikedByCurrentUser",
                table: "TouristSpots",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsLikedByCurrentUser",
                table: "TouristSpots");
        }
    }
}
