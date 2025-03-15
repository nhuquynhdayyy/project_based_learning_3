using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TourismWeb.Models
{
    public class Review
    {
        [Key]
        public int ReviewId { get; set; }

        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }

        public int? SpotId { get; set; }
        [ForeignKey("SpotId")]
        public TouristSpot Spot { get; set; }

        [Required]
        public int Rating { get; set; }

        public string Comment { get; set; }

        public string ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
