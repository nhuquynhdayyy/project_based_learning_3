using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TourismWeb.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }
        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [StringLength(100)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string AvatarUrl { get; set; } = "/images/default-avatar.png";

        public string Role { get; set; } = "User";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? LastLoginAt { get; set; }

        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Post> Posts { get; set; } = new List<Post>();
        public ICollection<PostComment> PostComments { get; set; } = new List<PostComment>();
        public ICollection<SpotFavorite> SpotFavorites { get; set; } = new List<SpotFavorite>();
        public ICollection<PostFavorite> PostFavorites { get; set; } = new List<PostFavorite>();
        public ICollection<SpotShare> SpotShares { get; set; } = new List<SpotShare>();
        public ICollection<PostShare> PostShares { get; set; } = new List<PostShare>();
        public ICollection<SpotImage> SpotImages { get; set; } = new List<SpotImage>();
        public ICollection<PostImage> PostImages { get; set; } = new List<PostImage>();
    }
}