using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TourismWeb.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; }

        public ICollection<TouristSpot> TouristSpots { get; set; } = new List<TouristSpot>();
    }
}