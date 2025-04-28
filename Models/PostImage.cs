using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace TourismWeb.Models
{
    public class PostImage
    {
        [Key]
        public int PostImageId { get; set; }

        [Required]
        [Display(Name = "Post")] 
        public int PostId { get; set; }
        [ValidateNever]
        public Post Post { get; set; }
        [Required]
        public string ImageUrl { get; set; } = "/images/default-postImage.png";
        [Required]
        [Display(Name = "User")] 
        public int UploadedBy { get; set; }
        [ValidateNever]
        public User User { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.Now;
    }
}