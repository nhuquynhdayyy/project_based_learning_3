using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TourismWeb.Models
{
    public class PostComment
    {
        [Key]
        public int CommentId { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public int PostId { get; set; }
        public Post Post { get; set; }

        [Required]
        public string Content { get; set; }

        public string ImageUrl { get; set; } = "";

        public string VideoUrl { get; set; } = "";

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}