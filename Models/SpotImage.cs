using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace TourismWeb.Models
{
    public class SpotImage
    {
        [Key]
        public int ImageId { get; set; }
        [Required]
        [Display(Name = "Spot")] 
        public int SpotId { get; set; }
        [ValidateNever]
        public TouristSpot Spot { get; set; }

        [Required]
        public string ImageUrl { get; set; } = "/images/default-spotImage.png";
        [Required]
        [Display(Name = "User")] 
        public int UploadedBy { get; set; }
        [ValidateNever]
        public User User { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.Now;
    }
}