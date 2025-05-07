using Microsoft.AspNetCore.Authorization;
// Trong AdminController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourismWeb.Models; // Namespace chứa ApplicationDbContext và các Model
using TourismWeb.Models.ViewModels; // Namespace chứa DashboardViewModel
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization; // Cho định dạng ngày tháng

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult ManageUsers()
    {
        return View();
    }

    // public IActionResult Dashboard()
    // {
    //     return View();
    // }
    public async Task<IActionResult> Dashboard(string timeRange = "30") // Mặc định là 30 ngày
    {
        var viewModel = new DashboardViewModel();
        DateTime startDate;

        // Xác định ngày bắt đầu dựa trên timeRange
        switch (timeRange)
        {
            case "7":
                startDate = DateTime.Today.AddDays(-6); // 7 ngày bao gồm ngày hiện tại
                break;
            case "all":
                startDate = DateTime.MinValue; // Lấy tất cả
                break;
            case "30":
            default:
                startDate = DateTime.Today.AddDays(-29); // 30 ngày bao gồm ngày hiện tại
                break;
        }

        // --- Lấy dữ liệu cho Stats Cards ---
        viewModel.TotalPosts = await _context.Posts
                                            .Where(p => p.CreatedAt >= startDate)
                                            .CountAsync();
        // Đếm số lượng TouristSpots (Địa điểm)
        viewModel.TotalTouristSpots = await _context.TouristSpots
                                             .Where(ts => ts.CreatedAt >= startDate)
                                             .CountAsync();

        // Đếm bài viết theo loại (Type) - Giả sử model Post có thuộc tính string Type
        // Bạn cần đảm bảo tên "Cẩm nang", "Trải nghiệm", "Địa điểm" khớp với giá trị trong DB
        viewModel.PostsInGuidebookCategory = await _context.Posts
                                                    .Where(p => p.TypeOfPost == "Cẩm nang" && p.CreatedAt >= startDate)
                                                    .CountAsync();
        viewModel.PostsInExperienceCategory = await _context.Posts
                                                     .Where(p => p.TypeOfPost == "Trải nghiệm" && p.CreatedAt >= startDate)
                                                     .CountAsync();
        viewModel.PostsInLocationCategory = await _context.Posts
                                                     .Where(p => p.TypeOfPost == "Địa điểm" && p.CreatedAt >= startDate)
                                                     .CountAsync();
        // Đếm số bài viết thuộc loại "Địa điểm" (Nếu "Địa điểm" cũng là một Type trong bảng Posts)
        // var postsInLocationCategory = await _context.Posts
        //                                      .Where(p => p.TypeOfPost == "Địa điểm" && p.CreatedAt >= startDate)
        //                                      .CountAsync();


        // --- Dữ liệu cho Biểu đồ Số bài viết trong X ngày qua ---
        var postsByDay = await _context.Posts
                                .Where(p => p.CreatedAt >= startDate)
                                .GroupBy(p => p.CreatedAt.Date) // Nhóm theo ngày
                                .Select(g => new { Date = g.Key, Count = g.Count() })
                                .OrderBy(x => x.Date)
                                .ToListAsync();

        foreach (var dayData in postsByDay)
        {
            viewModel.PostChartLabels.Add(dayData.Date.ToString("dd/MM")); // Format ngày
            viewModel.PostChartData.Add(dayData.Count);
        }


        // --- Dữ liệu cho Biểu đồ Tỷ lệ phân bố bài viết ---
        // Ví dụ: Phân bố theo Loại (Type) của bài viết
        var postsDistribution = await _context.Posts
                                    .Where(p => p.CreatedAt >= startDate) // Lọc theo thời gian nếu muốn
                                    .GroupBy(p => p.TypeOfPost) // Giả sử Post có thuộc tính Type
                                    .Select(g => new { TypeName = g.Key, Count = g.Count() })
                                    .ToListAsync();

        foreach (var distData in postsDistribution)
        {
            viewModel.DistributionChartLabels.Add(distData.TypeName ?? "Không xác định");
            viewModel.DistributionChartData.Add(distData.Count);
        }
        // Bạn cũng có thể thêm TouristSpots vào biểu đồ phân bố nếu muốn
        if (viewModel.TotalTouristSpots > 0) // Chỉ thêm nếu có TouristSpots
        {
             viewModel.DistributionChartLabels.Add("Địa điểm (Spots)");
             viewModel.DistributionChartData.Add(viewModel.TotalTouristSpots);
        }


        // --- Hoạt động gần đây (5 bài viết mới nhất) ---
        viewModel.RecentActivities = await _context.Posts
                                            .Include(p => p.User) // Để lấy tên tác giả
                                            .OrderByDescending(p => p.CreatedAt)
                                            .Take(5)
                                            .ToListAsync();

        return View(viewModel);
    }
    public IActionResult Posts()
    {
        return View();
    }
    public IActionResult Comments()
    {
        return View();
    }
    public IActionResult Interactions()
    {
        return View();
    }
    public IActionResult Reports()
    {
        return View();
    }
    public IActionResult Statistics()
    {
        return View();
    }
    public IActionResult Users()
    {
        return View();
    }
    public IActionResult Settings()
    {
        return View();
    }
}
