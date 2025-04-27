using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace TourismWeb.Models
{
    public class PostTag
    {
        [Required]
        [Display(Name = "Post")]
        public int PostId { get; set; }
        [ValidateNever]
        public Post Post { get; set; }
        [Required]
        [Display(Name = "Tag")]

        public int TagId { get; set; }
        [ValidateNever]
        public Tag Tag { get; set; }
    }
}