using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TourismWeb.Models
{
    public class Comment
    {
        [Key]
        public int CommentId { get; set; }

        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }

        public int? SpotId { get; set; }
        [ForeignKey("SpotId")]
        public TouristSpot Spot { get; set; }

        public int? PostId { get; set; }
        [ForeignKey("PostId")]
        public TouristSpot PostID { get; set; }

        [Required]
        public string Content { get; set; }

        public string ImageUrl { get; set; }
        public string VideoUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
