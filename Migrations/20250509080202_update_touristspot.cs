using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TourismWeb.Migrations
{
    /// <inheritdoc />
    public partial class update_touristspot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SpotComments");

            migrationBuilder.DropColumn(
                name: "IsLikedByCurrentUser",
                table: "TouristSpots");

            migrationBuilder.AddColumn<string>(
                name: "AvailableServices",
                table: "TouristSpots",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IdealVisitTime",
                table: "TouristSpots",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MapEmbedUrl",
                table: "TouristSpots",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TravelTips",
                table: "TouristSpots",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VideoEmbedUrl",
                table: "TouristSpots",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailableServices",
                table: "TouristSpots");

            migrationBuilder.DropColumn(
                name: "IdealVisitTime",
                table: "TouristSpots");

            migrationBuilder.DropColumn(
                name: "MapEmbedUrl",
                table: "TouristSpots");

            migrationBuilder.DropColumn(
                name: "TravelTips",
                table: "TouristSpots");

            migrationBuilder.DropColumn(
                name: "VideoEmbedUrl",
                table: "TouristSpots");

            migrationBuilder.AddColumn<bool>(
                name: "IsLikedByCurrentUser",
                table: "TouristSpots",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "SpotComments",
                columns: table => new
                {
                    CommentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SpotId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpotComments", x => x.CommentId);
                    table.ForeignKey(
                        name: "FK_SpotComments_TouristSpots_SpotId",
                        column: x => x.SpotId,
                        principalTable: "TouristSpots",
                        principalColumn: "SpotId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SpotComments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SpotComments_SpotId",
                table: "SpotComments",
                column: "SpotId");

            migrationBuilder.CreateIndex(
                name: "IX_SpotComments_UserId",
                table: "SpotComments",
                column: "UserId");
        }
    }
}
