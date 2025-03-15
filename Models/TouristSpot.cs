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

        [Required, MaxLength(255)]
        public string Address { get; set; }
        
        [Required, MaxLength(50)]
        public string Category { get; set; }

        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string OpeningHours { get; set; }
        public decimal? EntranceFee { get; set; }
        public string Services { get; set; }

        public int CreatedBy { get; set; }

        [ForeignKey("CreatedBy")]
        public User Creator { get; set; }

        public DateTime CreatedAt { get; set; }

        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
        // Thêm danh sách ảnh liên kết với TouristSpot
        public List<Image> Images { get; set; } = new List<Image>();
    }
}
