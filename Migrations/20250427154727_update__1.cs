using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TourismWeb.Migrations
{
    /// <inheritdoc />
    public partial class update__1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VideoUrl",
                table: "SpotComments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VideoUrl",
                table: "SpotComments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
