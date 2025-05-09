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
using System.Text.Json;
using System.Collections.Generic; // Required for List

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AdminController> _logger; // Thêm logger vào đây nếu cần thiết
    [ActivatorUtilitiesConstructor]
    public AdminController(ApplicationDbContext context, ILogger<AdminController> logger)
    {
        _context = context;
        _logger = logger; // Khởi tạo logger
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
    public async Task<IActionResult> Dashboard(string timeRange = "30")
{
    var viewModel = new DashboardViewModel();
    DateTime startDate;

    // Xác định ngày bắt đầu dựa trên timeRange
    switch (timeRange)
    {
        case "7":
            startDate = DateTime.Today.AddDays(-6);
            break;
        case "all":
            startDate = DateTime.MinValue;
            break;
        case "30":
        default:
            startDate = DateTime.Today.AddDays(-29);
            break;
    }

    // --- Lấy dữ liệu cho Stats Cards ---
    viewModel.TotalPosts = await _context.Posts
                                        .Where(p => p.CreatedAt >= startDate && p.Status == PostStatus.Approved)
                                        .CountAsync();

    // Số lượng này vẫn có thể hữu ích cho mục đích khác, nhưng không cho biểu đồ phân bố này
    // viewModel.TotalTouristSpots = await _context.TouristSpots
    //                                          .Where(ts => ts.CreatedAt >= startDate)
    //                                          .CountAsync();

    viewModel.PostsInGuidebookCategory = await _context.Posts
                                                .Where(p => p.TypeOfPost == "Cẩm nang" && p.CreatedAt >= startDate && p.Status == PostStatus.Approved)
                                                .CountAsync();
    viewModel.PostsInExperienceCategory = await _context.Posts
                                                 .Where(p => p.TypeOfPost == "Trải nghiệm" && p.CreatedAt >= startDate && p.Status == PostStatus.Approved)
                                                 .CountAsync();
    viewModel.PostsInLocationCategory = await _context.Posts // Đây là bài viết có TypeOfPost == "Địa điểm"
                                                 .Where(p => p.TypeOfPost == "Địa điểm" && p.CreatedAt >= startDate && p.Status == PostStatus.Approved)
                                                 .CountAsync();

    // --- Dữ liệu cho Biểu đồ Số bài viết trong X ngày qua ---
    var postsByDay = await _context.Posts
                            .Where(p => p.CreatedAt >= startDate)
                            .GroupBy(p => p.CreatedAt.Date)
                            .Select(g => new { Date = g.Key, Count = g.Count() })
                            .OrderBy(x => x.Date)
                            .ToListAsync();

    viewModel.PostChartLabels.Clear(); // Xóa dữ liệu cũ nếu có
    viewModel.PostChartData.Clear();   // Xóa dữ liệu cũ nếu có

    foreach (var dayData in postsByDay)
    {
        viewModel.PostChartLabels.Add(dayData.Date.ToString("dd/MM"));
        viewModel.PostChartData.Add(dayData.Count);
    }


    // --- Dữ liệu cho Biểu đồ Tỷ lệ phân bố bài viết ---
    // Xây dựng trực tiếp từ các giá trị PostsIn...Category đã tính
    viewModel.DistributionChartLabels.Clear(); // Xóa dữ liệu cũ nếu có
    viewModel.DistributionChartData.Clear();   // Xóa dữ liệu cũ nếu có

    // Tổng bài viết từ 3 loại
int totalPosts = viewModel.PostsInGuidebookCategory +
                 viewModel.PostsInExperienceCategory +
                 viewModel.PostsInLocationCategory;

// Tránh chia cho 0
if (totalPosts > 0)
{
    if (viewModel.PostsInGuidebookCategory > 0)
    {
        viewModel.DistributionChartLabels.Add("Cẩm nang");
        viewModel.DistributionChartData.Add(
            (int)Math.Round(viewModel.PostsInGuidebookCategory * 100.0 / totalPosts));
    }
    if (viewModel.PostsInExperienceCategory > 0)
    {
        viewModel.DistributionChartLabels.Add("Trải nghiệm");
        viewModel.DistributionChartData.Add(
            (int)Math.Round(viewModel.PostsInExperienceCategory * 100.0 / totalPosts));
    }
    if (viewModel.PostsInLocationCategory > 0)
    {
        viewModel.DistributionChartLabels.Add("Bài viết về Địa điểm");
        viewModel.DistributionChartData.Add(
            (int)Math.Round(viewModel.PostsInLocationCategory * 100.0 / totalPosts));
    }
}


    // Loại bỏ phần code truy vấn `postsDistribution` và thêm `TotalTouristSpots` vào biểu đồ, vì nó không còn cần thiết
    // cho yêu cầu này.

    // --- Hoạt động gần đây (5 bài viết mới nhất) ---
    viewModel.RecentActivities = await _context.Posts
                                        .Include(p => p.User)
                                        .OrderByDescending(p => p.CreatedAt)
                                        .Take(5)
                                        .ToListAsync();

    _logger.LogInformation($"PostsInGuidebookCategory: {viewModel.PostsInGuidebookCategory}");
_logger.LogInformation($"PostsInExperienceCategory: {viewModel.PostsInExperienceCategory}");
_logger.LogInformation($"PostsInLocationCategory: {viewModel.PostsInLocationCategory}");
_logger.LogInformation($"DistributionChartData before view: {JsonSerializer.Serialize(viewModel.DistributionChartData)}");

return View(viewModel);
}
    public IActionResult Posts()
    {
        return View();
    }
    // public IActionResult Comments()
    // {
    //     return View();
    // }
    public async Task<IActionResult> Comments()
{
    var reviews = await _context.Reviews
        .Include(r => r.User)
        .Include(r => r.Spot)
        .Select(r => new AdminCommentViewModel
        {
            Id = r.ReviewId,
            ItemType = "Review",
            UserFullName = r.User != null ? r.User.FullName : "N/A",
            UserEmail = r.User != null ? r.User.Email : "N/A",
            UserAvatar = r.User != null ? r.User.AvatarUrl : "/images/default-avatar.png",
            Content = r.Comment,
            RelatedItemId = r.SpotId,
            RelatedItemTitle = r.Spot.Name,
            RelatedItemController = "TouristSpots",
            RelatedItemTypeDetail = "Địa điểm du lịch (Đánh giá)", // GÁN GIÁ TRỊ
            Rating = r.Rating,
            CreatedAt = r.CreatedAt,
            ImageUrl = r.ImageUrl
        })
        .ToListAsync();

    var postComments = await _context.PostComments
        .Include(pc => pc.User)
        .Include(pc => pc.Post) // Đảm bảo Post được include
        .Select(pc => new AdminCommentViewModel
        {
            Id = pc.CommentId,
            ItemType = "PostComment",
            UserFullName = pc.User != null ? pc.User.FullName : "N/A",
            UserEmail = pc.User != null ? pc.User.Email : "N/A",
            UserAvatar = pc.User != null ? pc.User.AvatarUrl : "/images/default-avatar.png",
            Content = pc.Content,
            RelatedItemId = pc.PostId,
            RelatedItemTitle = pc.Post.Title,
            RelatedItemController = "Posts",
            RelatedItemTypeDetail = (pc.Post != null ? pc.Post.TypeOfPost : "Không rõ") + " (Bình luận)", // GÁN GIÁ TRỊ - Cần check pc.Post != null
            Rating = null,
            CreatedAt = pc.CreatedAt,
            ImageUrl = pc.ImageUrl
        })
        .ToListAsync();

    var allCommentsAndReviews = reviews.Concat(postComments)
                                       .OrderByDescending(c => c.CreatedAt)
                                       .ToList();

    return View(allCommentsAndReviews);
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
