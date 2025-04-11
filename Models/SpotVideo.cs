using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TourismWeb.Models
{
    public class SpotVideo
    {
        [Key]
        public int VideoId { get; set; }

        public int SpotId { get; set; }
        public TouristSpot Spot { get; set; }

        [Required]
        public string VideoUrl { get; set; } = "";

        public int UploadedBy { get; set; }
        public User User { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.Now;
    }
}