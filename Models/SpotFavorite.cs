using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TourismWeb.Models
{
    public class SpotFavorite
    {
        [Key]
        public int FavoriteId { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public int SpotId { get; set; }
        public TouristSpot Spot { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}