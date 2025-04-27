using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace TourismWeb.Models
{
    public class Review
    {
        [Key]
        public int ReviewId { get; set; }

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
        [Range(1, 5)]
        public int Rating { get; set; }
        [Required]
        public string Comment { get; set; } 

        public string ImageUrl { get; set; } = "/images/default-review.png";

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}