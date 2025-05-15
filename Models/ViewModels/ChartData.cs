// File: Models/ViewModels/ChartData.cs (hoặc vị trí tương tự)
using System.Collections.Generic; // Cần cho List<T>

namespace TourismWeb.Models.ViewModels
{
    public class ChartData
    {
        public List<string> Labels { get; set; }
        public List<ChartDataset> Datasets { get; set; }
        public string Title { get; set; } // Tiêu đề biểu đồ (có thể dùng hoặc không tùy cách bạn render)
    }
}