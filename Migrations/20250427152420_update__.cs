using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TourismWeb.Migrations
{
    /// <inheritdoc />
    public partial class update__ : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PostVideos");

            migrationBuilder.DropTable(
                name: "SpotVideos");

            migrationBuilder.DropColumn(
                name: "VideoUrl",
                table: "PostComments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VideoUrl",
                table: "PostComments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "PostVideos",
                columns: table => new
                {
                    PostVideoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostId = table.Column<int>(type: "int", nullable: false),
                    VideoUrl = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostVideos", x => x.PostVideoId);
                    table.ForeignKey(
                        name: "FK_PostVideos_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Posts",
                        principalColumn: "PostId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SpotVideos",
                columns: table => new
                {
                    VideoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SpotId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UploadedBy = table.Column<int>(type: "int", nullable: false),
                    VideoUrl = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpotVideos", x => x.VideoId);
                    table.ForeignKey(
                        name: "FK_SpotVideos_TouristSpots_SpotId",
                        column: x => x.SpotId,
                        principalTable: "TouristSpots",
                        principalColumn: "SpotId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SpotVideos_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PostVideos_PostId",
                table: "PostVideos",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_SpotVideos_SpotId",
                table: "SpotVideos",
                column: "SpotId");

            migrationBuilder.CreateIndex(
                name: "IX_SpotVideos_UserId",
                table: "SpotVideos",
                column: "UserId");
        }
    }
}
