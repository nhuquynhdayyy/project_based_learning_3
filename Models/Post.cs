using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TourismWeb.Models
{
    public class Post
    {
        [Key]
        public int PostId { get; set; }

        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }

        [Required, MaxLength(100)]
        public string Title { get; set; } // Tiêu đề bài viết

        [Required]
        public string Content { get; set; } // Nội dung bài viết

        public DateTime CreatedAt { get; set; } = DateTime.Now; // Ngày đăng bài
    }
}
