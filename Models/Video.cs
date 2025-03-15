using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TourismWeb.Models
{
    public class Video
    {
        [Key]
        public int VideoId { get; set; }

        public int SpotId { get; set; }
        [ForeignKey("SpotId")]
        public TouristSpot Spot { get; set; }

        [Required]
        public string VideoUrl { get; set; }

        public int UploadedBy { get; set; }
        [ForeignKey("UploadedBy")]
        public User Uploader { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.Now;
    }
}
