// File: Models/ViewModels/StatisticsViewModel.cs (hoặc vị trí tương tự)

namespace TourismWeb.Models.ViewModels
{
    public class StatisticsViewModel
    {
        public StatisticCardData NewUsersCard { get; set; }
        public StatisticCardData InteractionRateCard { get; set; }
        public ChartData TrafficChart { get; set; }
        public ChartData PostDistributionChart { get; set; } // Phân bố bài viết theo loại (Địa điểm, Cẩm nang, Trải nghiệm)
        public ChartData UserInteractionsChart { get; set; }
        public ChartData TopPostsChart { get; set; }
        public ChartData LocationDistributionChart { get; set; } // Phân bố bài viết theo địa điểm (Đà Lạt, Hội An,...)
    }
}