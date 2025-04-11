using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TourismWeb.Models
{
    public class SpotTag
    {
        public int SpotId { get; set; }
        public TouristSpot Spot { get; set; }

        public int TagId { get; set; }
        public Tag Tag { get; set; }
    }
}