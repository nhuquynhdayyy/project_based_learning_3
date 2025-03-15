using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TourismWeb.Models
{
    public class Image
    {
        [Key]
        public int ImageId { get; set; }

        [Required]
        public int SpotId { get; set; }
        
        [ForeignKey("SpotId")]
        public TouristSpot TouristSpot { get; set; }

        public int? UploadedBy { get; set; }  // Cho phép null

        [ForeignKey("UploadedBy")]
        public User Uploader { get; set; }  // Không cần Required để tránh lỗi khi xóa User

        [Required]
        public string ImageUrl { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.Now;
    }

}
