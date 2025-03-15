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

        // [MaxLength(20)]
        // public string PhoneNumber { get; set; }

        // public string AvatarUrl { get; set; }

        // [Required, MaxLength(20)]
        // public string Role { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool TwoFaEnabled { get; set; }
        // public string TwoFaSecret { get; set; }

         public string Role { get; set; } = "User";  // Giá trị mặc định

        public string AvatarUrl { get; set; } = "default-avatar.png";  // Giá trị mặc định

        public string PhoneNumber { get; set; } = "0000000000";  // Giá trị mặc định

        public string TwoFaSecret { get; set; } = "";  // Giá trị mặc định
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Share> Shares { get; set; } = new List<Share>();
        public ICollection<Image> Images { get; set; } = new List<Image>();
    }
}


