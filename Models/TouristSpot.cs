using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TourismWeb.Models
{
    public class TouristSpot
    {
        [Key]
        public int SpotId { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        [Required]
        public string Address { get; set; }

        [ForeignKey("Category")]
        public int CategoryId { get; set; }
        public Category Category { get; set; }

        public string Description { get; set; } = "";

        public string ImageUrl { get; set; } = "";

        public string VideoUrl { get; set; } = "";

        public double Latitude { get; set; } = 0.0;

        public double Longitude { get; set; } = 0.0;

        public string OpeningHours { get; set; } = "";

        [Column(TypeName = "decimal(18,2)")]
        public decimal? EntranceFee { get; set; }

        public string Services { get; set; } = "";

        [ForeignKey("User")]
        public int CreatedBy { get; set; }
        public User User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<SpotComment> Comments { get; set; } = new List<SpotComment>();
        public ICollection<SpotFavorite> Favorites { get; set; } = new List<SpotFavorite>();
        public ICollection<SpotShare> Shares { get; set; } = new List<SpotShare>();
        public ICollection<SpotImage> Images { get; set; } = new List<SpotImage>();
        public ICollection<SpotVideo> Videos { get; set; } = new List<SpotVideo>();
        public ICollection<Post> Posts { get; set; } = new List<Post>();
        public ICollection<SpotTag> SpotTags { get; set; } = new List<SpotTag>();
    }
}