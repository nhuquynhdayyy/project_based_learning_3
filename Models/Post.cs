using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TourismWeb.Models
{
    public class Post
    {
        [Key]
        public int PostId { get; set; }
        [Required]
        public int UserId { get; set; }
        // [Required]
        // public int SpotId { get; set; }
        // public int UserId { get; set; }
        // [ForeignKey("UserId")]
        // public User User { get; set; }

        // public int? SpotId { get; set; }
        // [ForeignKey("SpotId")]
        // public TouristSpot Spot { get; set; }
        // [ForeignKey("UserId")]
        // public User User { get; set; }
        public virtual User? User { get; set; }  // Đảm bảo nullable
        [Required(ErrorMessage = "Tiêu đề không được để trống"), MaxLength(100)]
        public string Title { get; set; } // Tiêu đề bài viết

        [Required(ErrorMessage = "Nội dung không được để trống")]
        public string Content { get; set; } // Nội dung bài viết

        public DateTime CreatedAt { get; set; } = DateTime.Now; // Ngày đăng bài
    }
}
