using System;
using System.Collections.Generic;
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
        public User User { get; set; }

        [Required]
        public int SpotId { get; set; }
        public TouristSpot Spot { get; set; }

        [Required, MaxLength(50)]
        public string TypeOfPost { get; set; } // VD: Địa điểm, Cẩm nang, Trải nghiệm

        [Required, MaxLength(100)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<PostImage> Images { get; set; } = new List<PostImage>();
        public ICollection<PostVideo> Videos { get; set; } = new List<PostVideo>();
        public ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
        public ICollection<PostFavorite> PostFavorites { get; set; } = new List<PostFavorite>();
        public ICollection<PostComment> Comments { get; set; } = new List<PostComment>();
        public ICollection<PostShare> Shares { get; set; } = new List<PostShare>();
    }
}