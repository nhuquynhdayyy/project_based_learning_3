using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace TourismWeb.Models
{
    public class PostFavorite
    {
        [Key]
        public int FavoriteId { get; set; }
        [Required]
        [Display(Name = "User")] 
        public int UserId { get; set; }
        [ValidateNever]
        public User User { get; set; }
        [Required]
        [Display(Name = "Post")] 
        public int PostId { get; set; }
        [ValidateNever]
        public Post Post { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}