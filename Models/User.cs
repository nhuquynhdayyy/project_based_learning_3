using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TourismWeb.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required, MaxLength(50)]
        public string Username { get; set; }

        [Required, MaxLength(100)]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [MaxLength(20)]
        public string PhoneNumber { get; set; } = "0000000000";

        public string AvatarUrl { get; set; } = "default-avatar.png";

        [MaxLength(20)]
        public string Role { get; set; } = "User";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? LastLoginAt { get; set; }

        public bool TwoFaEnabled { get; set; } = false;

        public string TwoFaSecret { get; set; } = "";

        public ICollection<TouristSpot> TouristSpots { get; set; } = new List<TouristSpot>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Post> Posts { get; set; } = new List<Post>();
        public ICollection<SpotComment> SpotComments { get; set; } = new List<SpotComment>();
        public ICollection<PostComment> PostComments { get; set; } = new List<PostComment>();
        public ICollection<SpotFavorite> SpotFavorites { get; set; } = new List<SpotFavorite>();
        public ICollection<PostFavorite> PostFavorites { get; set; } = new List<PostFavorite>();
        public ICollection<SpotShare> SpotShares { get; set; } = new List<SpotShare>();
        public ICollection<PostShare> PostShares { get; set; } = new List<PostShare>();
        public ICollection<SpotImage> SpotImages { get; set; } = new List<SpotImage>();
        public ICollection<SpotVideo> SpotVideos { get; set; } = new List<SpotVideo>();
        public ICollection<PostImage> PostImages { get; set; } = new List<PostImage>();
        public ICollection<PostVideo> PostVideos { get; set; } = new List<PostVideo>();
    }
}