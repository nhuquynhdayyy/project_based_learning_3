using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TourismWeb.Models
{
    public class Share
    {
        [Key]
        public int ShareId { get; set; }

        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }

        public int SpotId { get; set; }
        [ForeignKey("SpotId")]
        public TouristSpot Spot { get; set; }

        [Required, MaxLength(50)]
        public string SharedOn { get; set; }

        public DateTime SharedAt { get; set; } = DateTime.Now;
    }
}
