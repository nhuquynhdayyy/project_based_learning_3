using System.Collections.Generic;
using TourismWeb.Models;

namespace TourismWeb.Models.ViewModels
{
    public class SearchResultViewModel
    {
        public string SearchTerm { get; set; }
        public List<Post> Posts { get; set; } = new List<Post>();
        public List<TouristSpot> TouristSpots { get; set; } = new List<TouristSpot>();
    }
}