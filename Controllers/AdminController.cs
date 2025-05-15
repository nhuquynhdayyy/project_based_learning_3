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
using Microsoft.Extensions.Logging; // Thêm namespace cho ILogger
using System.Security.Claims; // Thêm namespace cho Claims

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
        var users = _context.Users.ToList(); // Example: Fetch users
        return View();
    }

    // public IActionResult Dashboard()
    // {
    //     return View();
    // }
    public async Task<IActionResult> Dashboard(string timeRange = "30", DateTime? fromDate = null, DateTime? toDate = null)
{
    var viewModel = new DashboardViewModel();
    DateTime startDate;

    // Kiểm tra xem có đang sử dụng bộ lọc tùy chỉnh không
    if (fromDate.HasValue && toDate.HasValue)
    {
        // Đảm bảo toDate không sớm hơn fromDate
        if (toDate < fromDate)
        {
            var temp = fromDate;
            fromDate = toDate;
            toDate = temp;
        }
        
        // Sử dụng khoảng thời gian tùy chỉnh
        startDate = fromDate.Value;
        
        // Thêm 1 ngày vào toDate để bao gồm cả ngày kết thúc
        toDate = toDate.Value.AddDays(1).AddTicks(-1);
        
        // Truyền giá trị đã chọn vào ViewBag
        ViewBag.FromDate = fromDate.Value.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate.Value.ToString("yyyy-MM-dd");
    }
    else
    {
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
        ViewBag.SelectedTimeRange = timeRange; // Truyền giá trị đã chọn vào ViewBag
    }

    // --- Lấy dữ liệu cho Stats Cards ---
    viewModel.TotalPosts = await _context.Posts
                                        .Where(p => (!fromDate.HasValue && !toDate.HasValue && p.CreatedAt >= startDate) || 
                                                   (fromDate.HasValue && toDate.HasValue && p.CreatedAt >= fromDate && p.CreatedAt <= toDate))
                                        .CountAsync();

    viewModel.PostsInGuidebookCategory = await _context.Posts
                                                .Where(p => p.TypeOfPost == "Cẩm nang" && 
                                                          ((!fromDate.HasValue && !toDate.HasValue && p.CreatedAt >= startDate) || 
                                                           (fromDate.HasValue && toDate.HasValue && p.CreatedAt >= fromDate && p.CreatedAt <= toDate)))
                                                .CountAsync();
                                                
    viewModel.PostsInExperienceCategory = await _context.Posts
                                                .Where(p => p.TypeOfPost == "Trải nghiệm" && 
                                                          ((!fromDate.HasValue && !toDate.HasValue && p.CreatedAt >= startDate) || 
                                                           (fromDate.HasValue && toDate.HasValue && p.CreatedAt >= fromDate && p.CreatedAt <= toDate)))
                                                .CountAsync();
                                                
    viewModel.PostsInLocationCategory = await _context.Posts
                                                .Where(p => p.TypeOfPost == "Địa điểm" && 
                                                          ((!fromDate.HasValue && !toDate.HasValue && p.CreatedAt >= startDate) || 
                                                           (fromDate.HasValue && toDate.HasValue && p.CreatedAt >= fromDate && p.CreatedAt <= toDate)))
                                                .CountAsync();

    // --- Dữ liệu cho Biểu đồ Số bài viết trong X ngày qua ---
    var postsByDay = await _context.Posts
                            .Where(p => (!fromDate.HasValue && !toDate.HasValue && p.CreatedAt >= startDate) || 
                                       (fromDate.HasValue && toDate.HasValue && p.CreatedAt >= fromDate && p.CreatedAt <= toDate))
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
    // public IActionResult Interactions()
    // {
    //     return View();
    // }
    public async Task<IActionResult> Interactions(string timeRange = "30", DateTime? fromDate = null, DateTime? toDate = null)
{
    // Convert timeRange to int for easier handling
    int days = 30; // Initialize with default value
    bool isAllTime = false;
    bool isCustomRange = false;
    
    // Kiểm tra xem có đang sử dụng bộ lọc tùy chỉnh không
    if (fromDate.HasValue && toDate.HasValue)
    {
        isCustomRange = true;
        // Đảm bảo toDate không sớm hơn fromDate
        if (toDate < fromDate)
        {
            var temp = fromDate;
            fromDate = toDate;
            toDate = temp;
        }
        // Thêm 1 ngày vào toDate để bao gồm cả ngày kết thúc
        toDate = toDate.Value.AddDays(1).AddTicks(-1);
    }
    else
    {
        switch (timeRange)
        {
            case "7":
                days = 7;
                break;
            case "-1":
                // Thay đổi ở đây: Thay vì đặt ngày bắt đầu từ năm 2000,
                // đặt ngày bắt đầu từ 12 tháng trước đến nay
                days = 365; // Khoảng 1 năm
                isAllTime = true;
                break;
            case "30":
            default:
                days = 30;
                break;
        }
        
        // Determine the start date based on selected time range
        fromDate = isAllTime 
            ? DateTime.Now.AddDays(-days) // Thay đổi ở đây: Thay vì new DateTime(2000, 1, 1)
            : DateTime.Now.AddDays(-days);
            
        toDate = DateTime.Now;
    }
    
    ViewBag.SelectedDays = isCustomRange ? 0 : (isAllTime ? -1 : int.Parse(timeRange));
    ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
    ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
        
    DateTime previousPeriodStart = isCustomRange
        ? fromDate.Value.AddDays(-(toDate.Value - fromDate.Value).Days)
        : (isAllTime ? fromDate.Value.AddDays(-days) : fromDate.Value.AddDays(-(toDate.Value - fromDate.Value).Days));
    
    DateTime previousPeriodEnd = isCustomRange
        ? fromDate.Value.AddTicks(-1)
        : (isAllTime ? fromDate.Value.AddTicks(-1) : fromDate.Value.AddTicks(-1));
    
    // Rest of the method remains unchanged
    // Tính tổng số lượt thích và chia sẻ trong khoảng thời gian đã chọn
    int totalPostFavorites = await _context.PostFavorites
        .Where(f => f.CreatedAt >= fromDate && f.CreatedAt <= toDate)
        .CountAsync();
        
    int totalPostShares = await _context.PostShares
        .Where(s => s.SharedAt >= fromDate && s.SharedAt <= toDate)
        .CountAsync();
        
    int totalSpotFavorites = await _context.SpotFavorites
        .Where(f => f.CreatedAt >= fromDate && f.CreatedAt <= toDate)
        .CountAsync();
        
    int totalSpotShares = await _context.SpotShares
        .Where(s => s.SharedAt >= fromDate && s.SharedAt <= toDate)
        .CountAsync();
        
    // Tính tổng số lượt thích và chia sẻ trong khoảng thời gian trước đó để tính tỷ lệ tăng trưởng
    int previousPostFavorites = await _context.PostFavorites
        .Where(f => f.CreatedAt >= previousPeriodStart && f.CreatedAt < previousPeriodEnd)
        .CountAsync();
        
    int previousPostShares = await _context.PostShares
        .Where(s => s.SharedAt >= previousPeriodStart && s.SharedAt < previousPeriodEnd)
        .CountAsync();
        
    int previousSpotFavorites = await _context.SpotFavorites
        .Where(f => f.CreatedAt >= previousPeriodStart && f.CreatedAt < previousPeriodEnd)
        .CountAsync();
        
    int previousSpotShares = await _context.SpotShares
        .Where(s => s.SharedAt >= previousPeriodStart && s.SharedAt < previousPeriodEnd)
        .CountAsync();
        
    // Tính tỷ lệ tăng trưởng
    ViewBag.PostFavoritesGrowth = previousPostFavorites > 0 
        ? Math.Round((double)(totalPostFavorites - previousPostFavorites) / previousPostFavorites * 100, 1)
        : 100;
        
    ViewBag.PostSharesGrowth = previousPostShares > 0 
        ? Math.Round((double)(totalPostShares - previousPostShares) / previousPostShares * 100, 1)
        : 100;
        
    ViewBag.SpotFavoritesGrowth = previousSpotFavorites > 0 
        ? Math.Round((double)(totalSpotFavorites - previousSpotFavorites) / previousSpotFavorites * 100, 1)
        : 100;
        
    ViewBag.SpotSharesGrowth = previousSpotShares > 0 
        ? Math.Round((double)(totalSpotShares - previousSpotShares) / previousSpotShares * 100, 1)
        : 100;
        
    // Lưu tổng số lượt thích và chia sẻ vào ViewBag
    ViewBag.TotalPostFavorites = totalPostFavorites;
    ViewBag.TotalPostShares = totalPostShares;
    ViewBag.TotalSpotFavorites = totalSpotFavorites;
    ViewBag.TotalSpotShares = totalSpotShares;
    
    // Tạo dữ liệu cho biểu đồ tương tác theo thời gian
    List<string> dateLabels = new List<string>();
    List<int> postFavoritesData = new List<int>();
    List<int> postSharesData = new List<int>();
    List<int> spotFavoritesData = new List<int>();
    List<int> spotSharesData = new List<int>();
    
    TimeSpan timeSpan = toDate.Value - fromDate.Value;
    int totalDays = (int)timeSpan.TotalDays;

    
    // Xác định khoảng thời gian cho biểu đồ
    if (totalDays <= 31) // Hiển thị theo ngày nếu dưới 31 ngày
    {
        // Lấy dữ liệu theo ngày
        for (DateTime date = fromDate.Value.Date; date <= toDate.Value.Date; date = date.AddDays(1))
        {
            DateTime nextDay = date.AddDays(1);
            
            dateLabels.Add(date.ToString("dd/MM"));
            
            postFavoritesData.Add(await _context.PostFavorites
                .Where(f => f.CreatedAt >= date && f.CreatedAt < nextDay)
                .CountAsync());
                
            postSharesData.Add(await _context.PostShares
                .Where(s => s.SharedAt >= date && s.SharedAt < nextDay)
                .CountAsync());
                
            spotFavoritesData.Add(await _context.SpotFavorites
                .Where(f => f.CreatedAt >= date && f.CreatedAt < nextDay)
                .CountAsync());
                
            spotSharesData.Add(await _context.SpotShares
                .Where(s => s.SharedAt >= date && s.SharedAt < nextDay)
                .CountAsync());
        }
    }
    else if (totalDays <= 90) // Hiển thị theo tuần nếu từ 31-90 ngày
    {
        // Tính toán ngày bắt đầu tuần (Thứ Hai)
        DateTime startOfWeek = fromDate.Value.Date.AddDays(-(int)fromDate.Value.DayOfWeek + 1);
        if ((int)fromDate.Value.DayOfWeek == 0) // Chủ nhật
            startOfWeek = fromDate.Value.Date.AddDays(-6);
            
        // Lấy dữ liệu theo tuần
        for (DateTime weekStart = startOfWeek; weekStart <= toDate.Value; weekStart = weekStart.AddDays(7))
        {
            DateTime weekEnd = weekStart.AddDays(7);
            
            dateLabels.Add($"{weekStart.ToString("dd/MM")} - {weekStart.AddDays(6).ToString("dd/MM")}");
            
            postFavoritesData.Add(await _context.PostFavorites
                .Where(f => f.CreatedAt >= weekStart && f.CreatedAt < weekEnd)
                .CountAsync());
                
            postSharesData.Add(await _context.PostShares
                .Where(s => s.SharedAt >= weekStart && s.SharedAt < weekEnd)
                .CountAsync());
                
            spotFavoritesData.Add(await _context.SpotFavorites
                .Where(f => f.CreatedAt >= weekStart && f.CreatedAt < weekEnd)
                .CountAsync());
                
            spotSharesData.Add(await _context.SpotShares
                .Where(s => s.SharedAt >= weekStart && s.SharedAt < weekEnd)
                .CountAsync());
        }
    }
    else // Hiển thị theo tháng nếu trên 90 ngày
    {
        // Tìm ngày đầu tiên của tháng
        DateTime startOfMonth = new DateTime(fromDate.Value.Year, fromDate.Value.Month, 1);
        
        // Lấy dữ liệu theo tháng
        for (DateTime monthStart = startOfMonth; monthStart <= toDate.Value; monthStart = monthStart.AddMonths(1))
        {
            DateTime monthEnd = monthStart.AddMonths(1);
            
            dateLabels.Add(monthStart.ToString("MM/yyyy"));
            
            postFavoritesData.Add(await _context.PostFavorites
                .Where(f => f.CreatedAt >= monthStart && f.CreatedAt < monthEnd)
                .CountAsync());
                
            postSharesData.Add(await _context.PostShares
                .Where(s => s.SharedAt >= monthStart && s.SharedAt < monthEnd)
                .CountAsync());
                
            spotFavoritesData.Add(await _context.SpotFavorites
                .Where(f => f.CreatedAt >= monthStart && f.CreatedAt < monthEnd)
                .CountAsync());
                
            spotSharesData.Add(await _context.SpotShares
                .Where(s => s.SharedAt >= monthStart && s.SharedAt < monthEnd)
                .CountAsync());
        }
    }
    
    ViewBag.DateLabels = dateLabels;
    ViewBag.PostFavoritesData = postFavoritesData;
    ViewBag.PostSharesData = postSharesData;
    ViewBag.SpotFavoritesData = spotFavoritesData;
    ViewBag.SpotSharesData = spotSharesData;
    
    // Lấy danh sách bài viết có nhiều tương tác nhất
    var topPosts = await _context.Posts
        .Select(p => new {
            p.PostId,
            p.Title,
            p.TypeOfPost,
            FavoritesCount = p.PostFavorites.Count(f => f.CreatedAt >= fromDate && f.CreatedAt <= toDate),
            SharesCount = p.Shares.Count(s => s.SharedAt >= fromDate && s.SharedAt <= toDate)
        })
        .OrderByDescending(p => p.FavoritesCount + p.SharesCount)
        .Take(5)
        .ToListAsync();
        
    ViewBag.TopPosts = topPosts;
    
    // Lấy danh sách địa điểm du lịch có nhiều tương tác nhất
    var topSpots = await _context.TouristSpots
        .Select(s => new {
            s.SpotId,
            s.Name,
            CategoryName = s.Category.Name,
            FavoritesCount = s.Favorites.Count(f => f.CreatedAt >= fromDate && f.CreatedAt <= toDate),
            SharesCount = s.Shares.Count(sh => sh.SharedAt >= fromDate && sh.SharedAt <= toDate)
        })
        .OrderByDescending(s => s.FavoritesCount + s.SharesCount)
        .Take(5)
        .ToListAsync();
        
    ViewBag.TopSpots = topSpots;
    
    // Lấy danh sách người dùng tương tác nhiều nhất
    var topUsers = await _context.Users
        .Select(u => new {
            u.UserId,
            u.FullName,
            u.Email,
            u.AvatarUrl,
            PostFavoritesCount = u.PostFavorites.Count(f => f.CreatedAt >= fromDate && f.CreatedAt <= toDate),
            PostSharesCount = u.PostShares.Count(s => s.SharedAt >= fromDate && s.SharedAt <= toDate),
            SpotFavoritesCount = u.SpotFavorites.Count(f => f.CreatedAt >= fromDate && f.CreatedAt <= toDate),
            SpotSharesCount = u.SpotShares.Count(s => s.SharedAt >= fromDate && s.SharedAt <= toDate)
        })
        .OrderByDescending(u => 
            u.PostFavoritesCount + u.PostSharesCount + u.SpotFavoritesCount + u.SpotSharesCount)
        .Take(5)
        .ToListAsync();
        
    ViewBag.TopUsers = topUsers;
    
    return View();
}
     // GET: Admin/Reports
    public async Task<IActionResult> Reports(string statusFilter = "Pending", string typeFilter = "all", string targetTypeFilter = "all", int pageNumber = 1, int pageSize = 10, string searchTerm = "")
    {
        ViewData["Title"] = "Quản Lý Báo Cáo - Hệ Thống Quản Trị";
        var query = _context.Reports
                            .Include(r => r.ReporterUser)
                            .Include(r => r.ReportedUser)
                            .AsQueryable();

        // Lọc theo trạng thái báo cáo
        if (!string.IsNullOrEmpty(statusFilter) && statusFilter.ToLower() != "all")
        {
            if (Enum.TryParse<ReportStatus>(statusFilter, true, out var parsedStatus))
            {
                query = query.Where(r => r.Status == parsedStatus);
            }
        }

        // Lọc theo loại báo cáo
        if (!string.IsNullOrEmpty(typeFilter) && typeFilter.ToLower() != "all")
        {
            if (Enum.TryParse<ReportType>(typeFilter, true, out var parsedType))
            {
                query = query.Where(r => r.TypeOfReport == parsedType);
            }
        }

        // Lọc theo đối tượng bị báo cáo
        if (!string.IsNullOrEmpty(targetTypeFilter) && targetTypeFilter.ToLower() != "all")
        {
            if (Enum.TryParse<ReportTargetType>(targetTypeFilter, true, out var parsedTargetType))
            {
                query = query.Where(r => r.TargetType == parsedTargetType);
            }
        }
        
        // Tìm kiếm
        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(r => r.Reason.Contains(searchTerm) ||
                                     (r.ReporterUser != null && r.ReporterUser.FullName.Contains(searchTerm)) ||
                                     (r.ReportedUser != null && r.ReportedUser.FullName.Contains(searchTerm)));
        }


        var totalReports = await query.CountAsync();
        var reports = await query
                                .OrderByDescending(r => r.ReportedAt)
                                .Skip((pageNumber - 1) * pageSize)
                                .Take(pageSize)
                                .ToListAsync();

        ViewBag.Reports = reports;
        ViewBag.TotalReports = totalReports;
        ViewBag.PageNumber = pageNumber;
        ViewBag.PageSize = pageSize;
        ViewBag.TotalPages = (int)Math.Ceiling(totalReports / (double)pageSize);
        ViewBag.CurrentStatusFilter = statusFilter;
        ViewBag.CurrentTypeFilter = typeFilter;
        ViewBag.CurrentTargetTypeFilter = targetTypeFilter;
        ViewBag.CurrentSearchTerm = searchTerm;

        return View();
    }

    // GET: Admin/GetReportDetails/5
    [HttpGet]
    public async Task<IActionResult> GetReportDetails(int id)
    {
        var report = await _context.Reports
                                   .Include(r => r.ReporterUser)
                                   .Include(r => r.ReportedUser)
                                   .FirstOrDefaultAsync(r => r.ReportId == id);

        if (report == null) return NotFound();

        object targetContent = null;
        string targetLink = "#";

        if (report.TargetType == ReportTargetType.Post && report.TargetId.HasValue)
        {
            var post = await _context.Posts.FindAsync(report.TargetId.Value);
            if (post != null)
            {
                targetContent = new { Type = "Post", Title = post.Title, Content = TruncateString(post.Content, 200) };
                targetLink = Url.Action("Details", "Posts", new { id = post.PostId }); // Điều chỉnh Controller/Action
            }
        }
        else if (report.TargetType == ReportTargetType.Comment && report.TargetId.HasValue)
        {
            var comment = await _context.PostComments.Include(c => c.Post).FirstOrDefaultAsync(c => c.CommentId == report.TargetId.Value);
            if (comment != null)
            {
                targetContent = new { Type = "Comment", Content = TruncateString(comment.Content, 200) };
                targetLink = Url.Action("Details", "Posts", new { id = comment.PostId }) + "#comment-" + comment.CommentId; // Điều chỉnh
            }
        }
        else if (report.TargetType == ReportTargetType.User && report.ReportedUserId.HasValue)
        {
             // Không có content cụ thể, chỉ là thông tin user
            targetContent = new { Type = "User" };
            targetLink = Url.Action("Details", "Users", new { id = report.ReportedUserId.Value }); // Giả sử có trang chi tiết user
        }


        var reportViewModel = new
        {
            report.ReportId,
            ReporterName = report.ReporterUser?.FullName,
            ReporterEmail = report.ReporterUser?.Email,
            ReporterAvatar = Url.Content(report.ReporterUser?.AvatarUrl ?? "~/images/default-avatar.png"),
            TypeOfReport = report.TypeOfReport.ToString(),
            TargetType = report.TargetType.ToString(),
            TargetId = report.TargetId,
            ReportedUserId = report.ReportedUserId,
            ReportedUserName = report.ReportedUser?.FullName,
            ReportedUserEmail = report.ReportedUser?.Email,
            ReportedUserAvatar = Url.Content(report.ReportedUser?.AvatarUrl ?? "~/images/default-avatar.png"),
            Reason = report.Reason,
            ReportedAt = report.ReportedAt.ToString("dd/MM/yyyy HH:mm"),
            Status = report.Status.ToString(), // Trạng thái hiện tại của báo cáo
            report.AdminNotes,
            TargetContent = targetContent,
            TargetLink = targetLink
        };

        return Json(reportViewModel);
    }

    // POST: Admin/ProcessReport
    [HttpPost]
    [ValidateAntiForgeryToken] // Quan trọng để chống CSRF
    public async Task<IActionResult> ProcessReport(int reportId, string newStatus, string adminAction, string adminNotes)
    {
        var report = await _context.Reports
                                   .Include(r => r.ReportedUser) // Để cập nhật UserStatus
                                   .FirstOrDefaultAsync(r => r.ReportId == reportId);

        if (report == null)
        {
            TempData["ErrorMessage"] = "Không tìm thấy báo cáo.";
            return RedirectToAction("Reports");
        }

        // Cập nhật thông tin xử lý báo cáo
        if (Enum.TryParse<ReportStatus>(newStatus, true, out var parsedStatus))
        {
            report.Status = parsedStatus;
        }
        else
        {
            TempData["ErrorMessage"] = "Trạng thái xử lý không hợp lệ.";
            return RedirectToAction("Reports");
        }

        var adminUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier); // Lấy ID admin đang đăng nhập
        if (int.TryParse(adminUserIdStr, out int adminId)) {
            report.AdminUserId = adminId;
        }
        
        report.ResolvedAt = DateTime.Now;
        report.AdminNotes = adminNotes;

        // Thực hiện hành động đối với người dùng hoặc nội dung
        if (report.ReportedUser != null) // Chỉ thực hiện nếu có người dùng liên quan trực tiếp
        {
            var targetUser = report.ReportedUser;
            bool userUpdated = false;

            switch (adminAction?.ToLower())
            {
                case "delete_content": // Giả sử có action này từ form
                    if (report.TargetType == ReportTargetType.Post && report.TargetId.HasValue)
                    {
                        var post = await _context.Posts.FindAsync(report.TargetId.Value);
                        if (post != null) _context.Posts.Remove(post);
                    }
                    else if (report.TargetType == ReportTargetType.Comment && report.TargetId.HasValue)
                    {
                        var comment = await _context.PostComments.FindAsync(report.TargetId.Value);
                        if (comment != null) _context.PostComments.Remove(comment);
                    }
                    break;
                case "warn_user":
                    // Logic cảnh báo (ví dụ: ghi log, gửi email - ngoài phạm vi code này)
                    // Có thể thêm một trường "WarningCount" vào User model
                    break;
                case "ban_user":
                    targetUser.UserStatus = "Bị cấm"; // Đảm bảo giá trị này khớp với logic hiển thị
                    _context.Users.Update(targetUser);
                    userUpdated = true;
                    break;
                case "ignore_report":
                    // Không làm gì với user/content, chỉ cập nhật trạng thái report
                    break;
            }
            if(userUpdated) await _context.SaveChangesAsync(); // Lưu thay đổi user trước
        }
        
        _context.Reports.Update(report);
        await _context.SaveChangesAsync(); // Lưu thay đổi report

        TempData["SuccessMessage"] = "Báo cáo đã được xử lý.";
        return RedirectToAction("Reports");
    }

    private string TruncateString(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value.Substring(0, maxLength) + "...";
    }
    public IActionResult Statistics()
    {
        return View();
    }
    [HttpGet]
        public async Task<IActionResult> GetStatisticsData(string timeRange = "30", string trafficInterval = "month")
        {
            // === LOGIC LẤY DỮ LIỆU TỪ DATABASE/SERVICES DỰA VÀO timeRange VÀ trafficInterval ===
            // Đây là dữ liệu giả lập, bạn cần thay thế bằng logic thực tế

            int days = timeRange == "all" ? 365 * 5 : int.Parse(timeRange);

            var newUsersCard = new StatisticCardData // Giả sử StatisticCardData đã được định nghĩa và using
            {
                Label = "Người dùng mới",
                Value = (days / 10 + 5).ToString(),
                TrendPercentage = $"+{days % 10 + 1}%",
                IsPositiveTrend = true,
                ComparisonText = "so với kỳ trước",
                IconClass = "fas fa-user-plus",
                IconColorClass = "green"
            };

            var interactionRateCard = new StatisticCardData
            {
                Label = "Tỷ lệ tương tác",
                Value = $"{20 + (days % 15)}.{days % 9}%",
                TrendPercentage = $"+{days % 3 + 0.5}%",
                IsPositiveTrend = true,
                ComparisonText = "so với kỳ trước",
                IconClass = "fas fa-chart-line",
                IconColorClass = "amber"
            };

            List<string> trafficLabels;
            List<double> trafficVisitorsData;
            List<double> trafficNewUsersData;
            List<double> trafficPostsData;

            if (trafficInterval == "day")
            {
                trafficLabels = Enumerable.Range(1, 7).Select(i => $"Ngày {i * (days / 7)}").ToList();
                trafficVisitorsData = trafficLabels.Select(l => (double)new System.Random().Next(1000, 3000)).ToList();
                trafficNewUsersData = trafficLabels.Select(l => (double)new System.Random().Next(200, 800)).ToList();
                trafficPostsData = trafficLabels.Select(l => (double)new System.Random().Next(10, 50)).ToList();
            }
            else if (trafficInterval == "week")
            {
                trafficLabels = Enumerable.Range(1, 4).Select(i => $"Tuần {i}").ToList();
                trafficVisitorsData = trafficLabels.Select(l => (double)new System.Random().Next(5000, 10000)).ToList();
                trafficNewUsersData = trafficLabels.Select(l => (double)new System.Random().Next(1000, 3000)).ToList();
                trafficPostsData = trafficLabels.Select(l => (double)new System.Random().Next(50, 150)).ToList();
            }
            else // month (default)
            {
                trafficLabels = new List<string> { "T1", "T2", "T3", "T4", "T5", "T6", "T7", "T8", "T9", "T10", "T11", "T12" }.Take(System.Math.Min(12, days / 25 + 1)).ToList();
                trafficVisitorsData = trafficLabels.Select(l => (double)new System.Random().Next(1000, 5000)).ToList();
                trafficNewUsersData = trafficLabels.Select(l => (double)new System.Random().Next(300, 1200)).ToList();
                trafficPostsData = trafficLabels.Select(l => (double)new System.Random().Next(10, 60)).ToList();
            }

            var trafficChart = new ChartData // Giả sử ChartData và ChartDataset đã được định nghĩa và using
            {
                Title = "Lượt xem, người dùng và bài viết theo thời gian",
                Labels = trafficLabels,
                Datasets = new List<ChartDataset>
                {
                    new ChartDataset { Label = "Lượt xem", Data = trafficVisitorsData, BorderColor = "#3b82f6", PointBackgroundColor = "#3b82f6", BackgroundColor = "transparent" },
                    new ChartDataset { Label = "Người dùng", Data = trafficNewUsersData, BorderColor = "#10b981", PointBackgroundColor = "#10b981", BackgroundColor = "transparent" },
                    new ChartDataset { Label = "Bài viết", Data = trafficPostsData, BorderColor = "#f59e0b", PointBackgroundColor = "#f59e0b", BackgroundColor = "transparent" }
                }
            };
            
            var postDistributionChart = new ChartData
            {
                Title = "Phân bố bài viết theo loại",
                Labels = new List<string> { "Địa điểm", "Cẩm nang", "Trải nghiệm" },
                Datasets = new List<ChartDataset>
                {
                    new ChartDataset
                    {
                        Data = new List<double> { new System.Random().Next(20, 50), new System.Random().Next(20, 50), new System.Random().Next(20, 50) },
                        BackgroundColors = new List<string> { "#3b82f6", "#10b981", "#f59e0b" }
                    }
                }
            };

            var userInteractionsLabels = new List<string> { "T1", "T2", "T3", "T4", "T5", "T6", "T7", "T8", "T9", "T10", "T11", "T12" }.Take(System.Math.Min(12, days / 25 + 1)).ToList();
            var userInteractionsChart = new ChartData
            {
                Title = "Tương tác người dùng theo thời gian",
                Labels = userInteractionsLabels,
                Datasets = new List<ChartDataset>
                {
                    new ChartDataset { Label = "Lượt xem", Data = userInteractionsLabels.Select(l => (double)new System.Random().Next(1000, 4000)).ToList(), BorderColor = "#3b82f6", BackgroundColor = "rgba(59, 130, 246, 0.1)", PointBackgroundColor = "#3b82f6", Fill = true },
                    new ChartDataset { Label = "Lượt thích", Data = userInteractionsLabels.Select(l => (double)new System.Random().Next(300, 1000)).ToList(), BorderColor = "#10b981", BackgroundColor = "transparent", PointBackgroundColor = "#10b981" },
                    new ChartDataset { Label = "Bình luận", Data = userInteractionsLabels.Select(l => (double)new System.Random().Next(100, 500)).ToList(), BorderColor = "#f59e0b", BackgroundColor = "transparent", PointBackgroundColor = "#f59e0b" },
                    new ChartDataset { Label = "Chia sẻ", Data = userInteractionsLabels.Select(l => (double)new System.Random().Next(50, 300)).ToList(), BorderColor = "#8b5cf6", BackgroundColor = "transparent", PointBackgroundColor = "#8b5cf6" }
                }
            };

            var topPostsData = new List<TopPostData> // Giả sử TopPostData đã được định nghĩa và using
            {
                new TopPostData { PostName = "Bài viết A", Views = new System.Random().Next(5000, 15000) },
                new TopPostData { PostName = "Bài viết B", Views = new System.Random().Next(4000, 12000) },
                new TopPostData { PostName = "Bài viết C", Views = new System.Random().Next(3000, 10000) },
                new TopPostData { PostName = "Bài viết D", Views = new System.Random().Next(2000, 8000) },
                new TopPostData { PostName = "Bài viết E", Views = new System.Random().Next(1000, 7000) }
            }.OrderByDescending(p => p.Views).ToList();

            var topPostsChart = new ChartData
            {
                Title = "Top 5 bài viết được xem nhiều nhất",
                Labels = topPostsData.Select(p => p.PostName).ToList(),
                Datasets = new List<ChartDataset>
                {
                    new ChartDataset
                    {
                        Label = "Lượt xem",
                        Data = topPostsData.Select(p => (double)p.Views).ToList(),
                        BackgroundColor = "#4a8af4",
                        BarThickness = 30,
                        BorderRadius = 4
                    }
                }
            };
            
            var locationData = new Dictionary<string, double>
            {
                {"Đà Lạt", new System.Random().Next(10, 20)}, {"Hội An", new System.Random().Next(10, 20)},
                {"Quảng Bình", new System.Random().Next(5, 15)}, {"Phú Quốc", new System.Random().Next(5, 15)},
                {"Hà Nội", new System.Random().Next(5, 10)}, {"Đà Nẵng", new System.Random().Next(5, 10)},
                {"Khác", new System.Random().Next(20, 40)}
            };
            var locationDistributionChart = new ChartData
            {
                Title = "Phân bố bài viết theo địa điểm",
                Labels = locationData.Keys.ToList(),
                Datasets = new List<ChartDataset>
                {
                    new ChartDataset
                    {
                        Data = locationData.Values.ToList(),
                        BackgroundColors = new List<string> { "#4a77ea", "#27c179", "#f89e24", "#f35b9f", "#8f7ee6", "#f44336", "#78909c" }
                    }
                }
            };

            var viewModel = new StatisticsViewModel // Giả sử StatisticsViewModel đã được định nghĩa và using
            {
                NewUsersCard = newUsersCard,
                InteractionRateCard = interactionRateCard,
                TrafficChart = trafficChart,
                PostDistributionChart = postDistributionChart,
                UserInteractionsChart = userInteractionsChart,
                TopPostsChart = topPostsChart,
                LocationDistributionChart = locationDistributionChart
            };

            return Json(viewModel);
        }
    // Action để hiển thị trang quản lý người dùng
    public async Task<IActionResult> Users(int pageNumber = 1, int pageSize = 10, string searchTerm = "", string roleFilter = "all", string statusFilter = "all")
    {
        // Truy vấn cơ bản, lấy tất cả User và include Posts để đếm
        var query = _context.Users.Include(u => u.Posts).AsQueryable();

        // TODO: Thêm logic lọc dựa trên searchTerm, roleFilter, statusFilter (sẽ phức tạp hơn)
        // Ví dụ đơn giản cho searchTerm:
        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(u => u.FullName.Contains(searchTerm) || u.Email.Contains(searchTerm) || u.Username.Contains(searchTerm));
        }

        // Ví dụ đơn giản cho roleFilter (giả sử Role là string):
        if (!string.IsNullOrEmpty(roleFilter) && roleFilter != "all")
        {
            query = query.Where(u => u.Role == roleFilter);
        }

        // Ví dụ đơn giản cho statusFilter (giả sử bạn có thuộc tính UserStatus):
        // Bạn cần thêm thuộc tính UserStatus vào model User
        // public string UserStatus { get; set; } // Ví dụ: "Active", "Inactive", "Banned"
        if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "all")
        {
            // query = query.Where(u => u.UserStatus == statusFilter); // Bỏ comment khi có UserStatus
        }

        var totalUsers = await query.CountAsync();
        var users = await query
                            .OrderByDescending(u => u.CreatedAt) // Sắp xếp theo người dùng mới nhất
                            .Skip((pageNumber - 1) * pageSize)
                            .Take(pageSize)
                            .ToListAsync();

        ViewBag.Users = users;
        ViewBag.TotalUsers = totalUsers;
        ViewBag.PageNumber = pageNumber;
        ViewBag.PageSize = pageSize;
        ViewBag.TotalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);

        // Truyền các giá trị filter hiện tại lại cho view để giữ trạng thái trên select box
        ViewBag.SearchTerm = searchTerm;
        ViewBag.RoleFilter = roleFilter;
        ViewBag.StatusFilter = statusFilter;

        return View();
    }
    public IActionResult Settings()
    {
        return View();
    }
}
