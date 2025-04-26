using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TourismWeb.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTouristSpot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PostVideos_Users_UserId",
                table: "PostVideos");

            migrationBuilder.DropForeignKey(
                name: "FK_SpotVideos_Users_UploadedBy",
                table: "SpotVideos");

            migrationBuilder.DropForeignKey(
                name: "FK_TouristSpots_Users_CreatedBy",
                table: "TouristSpots");

            migrationBuilder.DropIndex(
                name: "IX_TouristSpots_CreatedBy",
                table: "TouristSpots");

            migrationBuilder.DropIndex(
                name: "IX_SpotVideos_UploadedBy",
                table: "SpotVideos");

            migrationBuilder.DropIndex(
                name: "IX_PostVideos_UserId",
                table: "PostVideos");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TouristSpots");

            migrationBuilder.DropColumn(
                name: "EntranceFee",
                table: "TouristSpots");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "TouristSpots");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "TouristSpots");

            migrationBuilder.DropColumn(
                name: "OpeningHours",
                table: "TouristSpots");

            migrationBuilder.DropColumn(
                name: "Services",
                table: "TouristSpots");

            migrationBuilder.DropColumn(
                name: "VideoUrl",
                table: "TouristSpots");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "PostVideos");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "TouristSpots",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "SpotVideos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TouristSpots_UserId",
                table: "TouristSpots",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SpotVideos_UserId",
                table: "SpotVideos",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_SpotVideos_Users_UserId",
                table: "SpotVideos",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TouristSpots_Users_UserId",
                table: "TouristSpots",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SpotVideos_Users_UserId",
                table: "SpotVideos");

            migrationBuilder.DropForeignKey(
                name: "FK_TouristSpots_Users_UserId",
                table: "TouristSpots");

            migrationBuilder.DropIndex(
                name: "IX_TouristSpots_UserId",
                table: "TouristSpots");

            migrationBuilder.DropIndex(
                name: "IX_SpotVideos_UserId",
                table: "SpotVideos");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "TouristSpots");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "SpotVideos");

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "TouristSpots",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "EntranceFee",
                table: "TouristSpots",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "TouristSpots",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "TouristSpots",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "OpeningHours",
                table: "TouristSpots",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Services",
                table: "TouristSpots",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VideoUrl",
                table: "TouristSpots",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "PostVideos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TouristSpots_CreatedBy",
                table: "TouristSpots",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SpotVideos_UploadedBy",
                table: "SpotVideos",
                column: "UploadedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PostVideos_UserId",
                table: "PostVideos",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_PostVideos_Users_UserId",
                table: "PostVideos",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_SpotVideos_Users_UploadedBy",
                table: "SpotVideos",
                column: "UploadedBy",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TouristSpots_Users_CreatedBy",
                table: "TouristSpots",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
