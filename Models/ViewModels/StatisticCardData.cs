// File: Models/ViewModels/StatisticCardData.cs (hoặc vị trí tương tự)
// Hoặc bạn có thể không cần namespace nếu file được đặt trực tiếp trong thư mục gốc của Models
namespace TourismWeb.Models.ViewModels 
{
    public class StatisticCardData
    {
        public string Label { get; set; }
        public string Value { get; set; } // Có thể là int, double, string tùy theo dữ liệu
        public string TrendPercentage { get; set; } // Ví dụ: "+8%"
        public bool IsPositiveTrend { get; set; }
        public string ComparisonText { get; set; } // Ví dụ: "so với tháng trước"
        public string IconClass { get; set; } // Ví dụ: "fas fa-user-plus"
        public string IconColorClass { get; set; } // Ví dụ: "green"
    }
}