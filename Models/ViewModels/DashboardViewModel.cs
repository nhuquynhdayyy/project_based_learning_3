// Trong DashboardViewModel.cs
using System.Collections.Generic;
using TourismWeb.Models; // Giả sử Post và các model khác nằm ở đây

namespace TourismWeb.Models.ViewModels // Hoặc Namespace phù hợp
{
    public class DashboardViewModel
    {
        // Stats Cards
        public int TotalPosts { get; set; }
        public int TotalTouristSpots { get; set; } // Nếu bạn muốn hiển thị số lượng địa điểm
        public int PostsInGuidebookCategory { get; set; } // "Cẩm nang du lịch"
        public int PostsInExperienceCategory { get; set; } // "Trải nghiệm"
        public int PostsInLocationCategory { get; set; } // "Trải nghiệm"
        // Thêm các thống kê khác nếu cần

        // Dữ liệu cho biểu đồ số bài viết theo ngày
        public List<string> PostChartLabels { get; set; } = new List<string>(); // Ngày
        public List<int> PostChartData { get; set; } = new List<int>();       // Số lượng bài viết

        // Dữ liệu cho biểu đồ phân bố bài viết
        public List<string> DistributionChartLabels { get; set; } = new List<string>(); // Tên loại/danh mục
        public List<int> DistributionChartData { get; set; } = new List<int>();       // Số lượng

        // Hoạt động gần đây (ví dụ: 5 bài viết mới nhất)
        public List<Post> RecentActivities { get; set; } = new List<Post>();

        public DashboardViewModel()
        {
            // Khởi tạo các List để tránh null reference
            PostChartLabels = new List<string>();
            PostChartData = new List<int>();
            DistributionChartLabels = new List<string>();
            DistributionChartData = new List<int>();
            RecentActivities = new List<Post>();
        }
    }
}