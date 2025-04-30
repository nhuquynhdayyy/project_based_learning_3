using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace TourismWeb.Models
{
    public class Post
    {
        [Key]
        public int PostId { get; set; }

        [Required]
        public int UserId { get; set; }
        [ValidateNever]
        public User User { get; set; }

        [Required]
        [Display(Name = "Spot")] 
        public int SpotId { get; set; }
        [ValidateNever]
        public TouristSpot Spot { get; set; }

        [Required]
        [MaxLength(50)]
        [RegularExpression("^(Địa điểm|Cẩm nang|Trải nghiệm)$", ErrorMessage = "Loại bài viết chỉ được chọn: Địa điểm, Cẩm nang hoặc Trải nghiệm.")]
        public string TypeOfPost { get; set; }


        [Required, MaxLength(100)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }
        public string ImageUrl { get; set; } = "/images/default-postImage.png" ;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Add status field for approval workflow
        [Display(Name = "Status")]
        public PostStatus Status { get; set; } = PostStatus.Pending;

        public ICollection<PostImage> Images { get; set; } = new List<PostImage>();
        public ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
        public ICollection<PostFavorite> PostFavorites { get; set; } = new List<PostFavorite>();
        public ICollection<PostComment> Comments { get; set; } = new List<PostComment>();
        public ICollection<PostShare> Shares { get; set; } = new List<PostShare>();
    }
    // Enum for post status
    public enum PostStatus
    {
        Pending,
        Approved,
        Rejected
    }
}