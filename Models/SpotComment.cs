using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace TourismWeb.Models
{
    public class SpotComment
    {
        [Key]
        public int CommentId { get; set; }
        [Required]
        [Display(Name = "User")] 
        public int UserId { get; set; }
        [ValidateNever]
        public User User { get; set; }
        [Required]
        [Display(Name = "Spot")] 

        public int SpotId { get; set; }
        [ValidateNever]
        public TouristSpot Spot { get; set; }

        [Required]
        public string Content { get; set; }

        public string ImageUrl { get; set; } = "/images/default-spotComment.png";

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}