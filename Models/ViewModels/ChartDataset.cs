// File: Models/ViewModels/ChartDataset.cs (hoặc vị trí tương tự)
using System.Collections.Generic; // Cần cho List<T>

namespace TourismWeb.Models.ViewModels 
{
    public class ChartDataset
    {
        public string Label { get; set; }
        public List<double> Data { get; set; } // Hoặc List<int>
        public string BorderColor { get; set; }
        public string BackgroundColor { get; set; } // Dùng cho line chart area fill hoặc bar chart
        public double Tension { get; set; } = 0.4;
        public int BorderWidth { get; set; } = 2;
        public int PointRadius { get; set; } = 5;
        public string PointBackgroundColor { get; set; }
        public bool Fill { get; set; } = false; // Cho line chart area fill
        public List<string> BackgroundColors { get; set; } // Cho pie/doughnut chart, hoặc từng bar trong bar chart
        public int? BarThickness { get; set; } // Cho bar chart (nullable nếu không phải lúc nào cũng dùng)
        public int? BorderRadius { get; set; } // Cho bar chart (nullable)
    }
}