using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TourismWeb.Models
{
    public class Tag
    {
        [Key]
        public int TagId { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; }

        public ICollection<SpotTag> SpotTags { get; set; } = new List<SpotTag>();
        public ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
    }
}