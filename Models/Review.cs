using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TourismWeb.Models
{
    public class Review
    {
        [Key]
        public int ReviewId { get; set; }

        [Required]
        public int UserId { get; set; }
        public User User { get; set; }

        [Required]
        public int SpotId { get; set; }
        public TouristSpot Spot { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        public string Comment { get; set; } // = "";

        public string ImageUrl { get; set; } = "/images/default-review.png";

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}