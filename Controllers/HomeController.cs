using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourismWeb.Models;
using TourismWeb.Models.ViewModels;

namespace TourismWeb.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    // public HomeController(ILogger<HomeController> logger)
    // {
    //     _logger = logger;
    // }
    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        // Lấy các đánh giá để hiển thị trên trang chủ/trang giới thiệu
        var customerReviews = await _context.Reviews
                                    .Include(r => r.User) // Để lấy tên và ảnh đại diện người dùng
                                    .Include(r => r.Spot) // Để lấy tên địa điểm được đánh giá
                                    .OrderByDescending(r => r.CreatedAt) // Sắp xếp theo mới nhất
                                    .Take(3) // Lấy 3 đánh giá hàng đầu (hoặc số lượng bạn muốn)
                                    .Select(r => new ReviewViewModel // Ánh xạ sang ReviewViewModel
                                    {
                                        // Id = r.ReviewId, // ViewModel của bạn không có Id, không sao cả
                                        AvatarImageUrl = r.User.AvatarUrl ?? "/images/avatar-default.png", // Lấy AvatarUrl từ User, nếu null thì dùng ảnh mặc định
                                        Rating = r.Rating,
                                        Comment = r.Comment,
                                        AuthorName = r.User.FullName, // Lấy FullName từ User
                                        TourName = r.Spot.Name, // Lấy Name từ Spot
                                        // CreatedAt = r.CreatedAt // ViewModel của bạn không có CreatedAt
                                    })
                                    .ToListAsync();

        ViewBag.CustomerReviews = customerReviews;

        // Get recent posts from all categories for the slider section
        var recentPosts = await _context.Posts
            .Include(p => p.User)
            .Include(p => p.Spot)
            .OrderByDescending(p => p.CreatedAt)
            .Take(8)  // Limiting to 8 posts for the slider
            .ToListAsync();
        // Lấy 3 bài viết cẩm nang mới nhất
        var guidebookPosts = await _context.Posts
            .Include(p => p.Spot)
            .Include(p => p.User)
            .Where(p => p.TypeOfPost == "Cẩm nang")
            .OrderByDescending(p => p.CreatedAt)
            .Take(3)
            .ToListAsync();
        
        // Lấy 6 địa điểm được thêm vào gần nhất
        var recentSpots = await _context.TouristSpots
                                        .OrderByDescending(spot => spot.CreatedAt) // Sắp xếp theo ngày tạo giảm dần
                                        .Take(6)                                 // Chỉ lấy 6 địa điểm đầu tiên
                                        .ToListAsync();

        // Truyền danh sách này đến View
        // Bạn có thể truyền qua ViewModel của View Index, hoặc qua ViewBag/ViewData nếu View đơn giản
        // Ví dụ sử dụng ViewBag:
        ViewBag.RecentTouristSpots = recentSpots;
        // Gửi dữ liệu đến view
        ViewBag.RecentPosts = recentPosts;
        ViewBag.GuidebookPosts = guidebookPosts;

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public IActionResult AccessDenied()
    {
        return View();
    }
    public IActionResult Contact()
    {
        return View();
    }
}
