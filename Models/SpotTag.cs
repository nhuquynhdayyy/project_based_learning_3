using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace TourismWeb.Models
{
    public class SpotTag
    {
        [Required]
        [Display(Name = "Spot")]
        public int SpotId { get; set; }
        [ValidateNever]
        public TouristSpot Spot { get; set; }
        [Required]
        [Display(Name = "Tag")]
        public int TagId { get; set; }
        [ValidateNever]
        public Tag Tag { get; set; }
    }
}