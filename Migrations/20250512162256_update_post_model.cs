using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TourismWeb.Migrations
{
    /// <inheritdoc />
    public partial class update_post_model : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Bio",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Advice",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApproximateCost",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Companions",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EstimatedCosts",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EstimatedVisitTime",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExperienceEndDate",
                table: "Posts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExperienceHighlights",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExperienceItinerarySummary",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GuidebookSummary",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "LocationRating",
                table: "Posts",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "OverallExperienceRating",
                table: "Posts",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PackingListSuggestions",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "RatingFood",
                table: "Posts",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "RatingLandscape",
                table: "Posts",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "RatingPrice",
                table: "Posts",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "RatingService",
                table: "Posts",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SuggestedItinerary",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TicketPriceInfo",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TravelTips",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UsefulDocumentsHtml",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Bio",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Advice",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "ApproximateCost",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "Companions",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "EstimatedCosts",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "EstimatedVisitTime",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "ExperienceEndDate",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "ExperienceHighlights",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "ExperienceItinerarySummary",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "GuidebookSummary",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "LocationRating",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "OverallExperienceRating",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "PackingListSuggestions",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "RatingFood",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "RatingLandscape",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "RatingPrice",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "RatingService",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "SuggestedItinerary",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "TicketPriceInfo",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "TravelTips",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "UsefulDocumentsHtml",
                table: "Posts");
        }
    }
}
