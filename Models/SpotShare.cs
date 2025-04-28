using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace TourismWeb.Models
{
    public class SpotShare
    {
        [Key]
        public int ShareId { get; set; }
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

        [Required(ErrorMessage = "Vui lòng chọn nền tảng chia sẻ.")]
        [RegularExpression("^(Facebook|Twitter|Instagram|Email)$", ErrorMessage = "Chỉ được chọn: Facebook, Twitter, Instagram hoặc Email.")]
        [Display(Name = "Shared On")]
        public string SharedOn { get; set; } 

        public DateTime SharedAt { get; set; } = DateTime.Now;
    }
}