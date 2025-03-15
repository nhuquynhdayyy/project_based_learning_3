using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TourismWeb.Models
{
    public class Favorite
    {
        [Key]
        public int FavoriteId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int SpotId { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public User User { get; set; }
        public TouristSpot TouristSpot { get; set; }
    }
}
