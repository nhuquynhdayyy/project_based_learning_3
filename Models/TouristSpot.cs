using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace TourismWeb.Models
{
    public class TouristSpot
    {
        [Key]
        public int SpotId { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        [Required]
        public string Address { get; set; }
        [Required]
        [ForeignKey("Category")]
        [Display(Name = "Category")] 
        public int? CategoryId { get; set; }
        [ValidateNever]
        public Category Category { get; set; }

        public string Description { get; set; } 

        public string ImageUrl { get; set; } = "/images/default-spotImage.png" ;    
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsLikedByCurrentUser { get; set; } = false;

        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<SpotFavorite> Favorites { get; set; } = new List<SpotFavorite>();
        public ICollection<SpotShare> Shares { get; set; } = new List<SpotShare>();
        public ICollection<SpotImage> Images { get; set; } = new List<SpotImage>();
        public ICollection<Post> Posts { get; set; } = new List<Post>();
        public ICollection<SpotTag> SpotTags { get; set; } = new List<SpotTag>();
    }
}