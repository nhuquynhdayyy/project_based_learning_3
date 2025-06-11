using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourismWeb.Models; 
using TourismWeb.Models.ViewModels; 
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;
using System.Text.Json;
using System.Collections.Generic;
using Microsoft.Extensions.Logging; 
using System.Security.Claims; 
using Microsoft.Extensions.Configuration;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AdminController> _logger; 
    private readonly IConfiguration _configuration;
    [ActivatorUtilitiesConstructor]
    public AdminController(ApplicationDbContext context, ILogger<AdminController> logger, IConfiguration configuration)
    {
        _context = context;
        _logger = logger; 
        _configuration = configuration;
    }
    private int GetCurrentAdminUserId()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(userIdString, out int userId))
        {
            return userId;
        }
        throw new Exception("Không thể xác định User ID của admin.");
    }
    
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult ManageUsers()
    {
        var users = _context.Users.ToList(); 
        return View();
    }

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
            RelatedItemTypeDetail = (pc.Post != null ? pc.Post.TypeOfPost : "Không rõ") + " (Bình luận)", 
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
            ? DateTime.Now.AddDays(-days)
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

        // return View();
        return View(query.ToList());
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
                targetLink = Url.Action("Details", "Posts", new { id = post.PostId });
            }
        }
        else if (report.TargetType == ReportTargetType.Comment && report.TargetId.HasValue)
        {
            var comment = await _context.PostComments
                                        .Include(c => c.User)
                                        .Include(c => c.Post)
                                        .FirstOrDefaultAsync(c => c.CommentId == report.TargetId.Value);
            if (comment != null)
            {
                targetContent = new { Type = "Comment", Content = TruncateString(comment.Content, 200) };
                targetLink = Url.Action("Details", "Posts", new { id = comment.PostId }) + "#comment-" + comment.CommentId;
            }
        }
        else if (report.TargetType == ReportTargetType.User && report.ReportedUserId.HasValue)
        {
            targetContent = new { Type = "User" }; // Có thể thêm thông tin user nếu muốn
            targetLink = Url.Action("Details", "Users", new { id = report.ReportedUserId.Value }); // Giả sử
        }


        var reportViewModel = new
        {
            report.ReportId,
            ReporterName = report.ReporterUser?.FullName,
            ReporterEmail = report.ReporterUser?.Email,
            ReporterAvatar = Url.Content(report.ReporterUser?.AvatarUrl ?? "~/images/default-avatar.png"),
            TypeOfReport = report.TypeOfReport?.ToString(), 
            TargetType = report.TargetType?.ToString(), 
            TargetId = report.TargetId,
            ReportedUserId = report.ReportedUserId,
            ReportedUserName = report.ReportedUser?.FullName,
            ReportedUserEmail = report.ReportedUser?.Email,
            ReportedUserAvatar = Url.Content(report.ReportedUser?.AvatarUrl ?? "~/images/default-avatar.png"),
            Reason = report.Reason,
            ReportedAt = report.ReportedAt.ToString("dd/MM/yyyy HH:mm"),
            Status = report.Status.ToString(),
            AdminNotes = report.AdminNotes,
            TargetContent = targetContent,
            TargetLink = targetLink
        };

        return Json(reportViewModel);
    }

    // POST: Admin/ProcessReport
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProcessReport(int reportId, string newStatus, string adminAction, string adminNotes, int? reportedUserIdToActOn) // reportedUserIdToActOn hiện không được sử dụng, cân nhắc bỏ nếu không cần
    {
        var report = await _context.Reports
                                   .Include(r => r.ReportedUser)
                                   .FirstOrDefaultAsync(r => r.ReportId == reportId);

        if (report == null)
        {
            TempData["ErrorMessage"] = "Không tìm thấy báo cáo.";
            return RedirectToAction("Reports");
        }

        // Cập nhật thông tin cơ bản của report
        var adminUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(adminUserIdStr, out int adminId))
        {
            report.AdminUserId = adminId;
        }
        report.ResolvedAt = DateTime.UtcNow; // Nên dùng UtcNow cho thời gian server
        report.AdminNotes = adminNotes;

        ReportStatus statusToSet; // Trạng thái cuối cùng sẽ được gán cho report
        bool userActionTaken = false; // Cờ đánh dấu nếu có hành động lên người dùng
        bool contentActionTaken = false; // Cờ đánh dấu nếu có hành động lên nội dung

        // 1. Xử lý hành động liên quan đến nội dung (ưu tiên, vì có thể độc lập với ReportedUser)
        if (adminAction?.ToLower() == "delete_content")
        {
            contentActionTaken = true;
            if (report.TargetType == ReportTargetType.Post && report.TargetId.HasValue)
            {
                var post = await _context.Posts.FindAsync(report.TargetId.Value);
                if (post != null)
                {
                    _context.Posts.Remove(post);
                    TempData["InfoMessage"] = $"Đã xóa bài viết (ID: {post.PostId}).";
                }
                else
                {
                    TempData["WarningMessage"] = "Không tìm thấy bài viết để xóa.";
                }
            }
            else if (report.TargetType == ReportTargetType.Comment && report.TargetId.HasValue)
            {
                var comment = await _context.PostComments.FindAsync(report.TargetId.Value);
                if (comment != null)
                {
                    _context.PostComments.Remove(comment);
                    TempData["InfoMessage"] = $"Đã xóa bình luận (ID: {comment.CommentId}).";
                }
                else
                {
                    TempData["WarningMessage"] = "Không tìm thấy bình luận để xóa.";
                }
            }
            else if (report.TargetType == ReportTargetType.Review && report.TargetId.HasValue)
            {
                var review = await _context.Reviews.FindAsync(report.TargetId.Value);
                if (review != null)
                {
                    _context.Reviews.Remove(review);
                    TempData["InfoMessage"] = $"Đã xóa bình luận (ID: {review.ReviewId}).";
                }
                else
                {
                    TempData["WarningMessage"] = "Không tìm thấy bình luận để xóa.";
                }
            }
            else
            {
                TempData["WarningMessage"] = "Hành động xóa nội dung không áp dụng cho loại đối tượng này.";
                contentActionTaken = false; // Không có gì để xóa thực sự
            }
        }

        // 2. Xử lý hành động liên quan đến người dùng (ReportedUser)
        // reportedUserIdToActOn không được dùng, nếu cần thì phải lấy user dựa trên ID này
        var userToActUpon = report.ReportedUser; // Mặc định hành động trên người dùng trong report

        if (userToActUpon != null)
        {
            switch (adminAction?.ToLower())
            {
                case "warn_user":
                    userActionTaken = true;
                    // Logic cảnh báo (ví dụ: userToActUpon.WarningCount++;)
                    // _context.Users.Update(userToActUpon); // Nếu có thay đổi db cho user
                    TempData["InfoMessage"] = (TempData["InfoMessage"]?.ToString() ?? "") + $" Đã ghi nhận cảnh báo cho người dùng: {userToActUpon.FullName ?? userToActUpon.Username}.";
                    break;
                case "ban_user":
                    userActionTaken = true;
                    userToActUpon.UserStatus = "Bị cấm"; // Cân nhắc dùng Enum hoặc Constant
                    // _context.Users.Update(userToActUpon);
                    TempData["InfoMessage"] = (TempData["InfoMessage"]?.ToString() ?? "") + $" Đã cấm người dùng: {userToActUpon.FullName ?? userToActUpon.Username}.";
                    break;
            }
        }
        else if (adminAction?.ToLower() == "warn_user" || adminAction?.ToLower() == "ban_user")
        {
            TempData["WarningMessage"] = (TempData["WarningMessage"]?.ToString() ?? "") + " Không thể thực hiện hành động cảnh báo/cấm vì báo cáo không gắn với người dùng cụ thể.";
        }


        // 3. Xác định trạng thái cuối cùng của báo cáo
        if (Enum.TryParse<ReportStatus>(newStatus, true, out var parsedNewStatus))
        {
            statusToSet = parsedNewStatus;
            // Nếu có hành động cụ thể (xóa, cấm, cảnh báo), thường report sẽ là Resolved
            if ((contentActionTaken || userActionTaken) && statusToSet == ReportStatus.Pending)
            {
            }
        }
        else
        {
            TempData["ErrorMessage"] = "Trạng thái xử lý không hợp lệ. Giữ nguyên trạng thái cũ.";
            statusToSet = report.Status; // Giữ nguyên trạng thái cũ nếu parse lỗi
        }
        
        // Nếu không có hành động nào được chọn nhưng trạng thái là Pending, và admin chọn ignore_report
        // thì trạng thái sẽ được cập nhật theo newStatus (đã xử lý ở trên)
        // Nếu admin chọn "ignore_report" và newStatus là "Resolved" hoặc "Dismissed" thì đó là lựa chọn của họ.
        if (adminAction?.ToLower() == "ignore_report" && !contentActionTaken && !userActionTaken)
        {
             // Không làm gì thêm, trạng thái đã được set từ newStatus
        }


        report.Status = statusToSet;
        _context.Reports.Update(report);

        try
        {
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Báo cáo đã được xử lý thành công.";
        }
        catch (DbUpdateException ex)
        {
            // Log lỗi (ex)
            TempData["ErrorMessage"] = "Đã xảy ra lỗi khi lưu thay đổi vào cơ sở dữ liệu.";
        }

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
public async Task<IActionResult> Users(int pageNumber = 1, int pageSize = 10, string searchTerm = "", string roleFilter = "all", string statusFilter = "all")
{
    // === BƯỚC 1: ÁP DỤNG CÁC BỘ LỌC CÓ THỂ TRÊN CSDL ===
    var query = _context.Users.Include(u => u.Posts).AsQueryable();

    // Lọc theo từ khóa tìm kiếm
    if (!string.IsNullOrEmpty(searchTerm))
    {
        var lowerCaseSearchTerm = searchTerm.ToLower().Trim();
        query = query.Where(u => u.FullName.ToLower().Contains(lowerCaseSearchTerm) ||
                                 u.Email.ToLower().Contains(lowerCaseSearchTerm) ||
                                 u.Username.ToLower().Contains(lowerCaseSearchTerm));
    }

    // Lọc theo vai trò
    if (!string.IsNullOrEmpty(roleFilter) && roleFilter.ToLower() != "all")
    {
        query = query.Where(u => u.Role.ToLower() == roleFilter.ToLower());
    }

    // === BƯỚC 2: LẤY DỮ LIỆU ĐÃ LỌC SƠ BỘ VÀO BỘ NHỚ ===
    // .ToList() sẽ thực thi câu lệnh SQL và tải kết quả vào bộ nhớ
    var allFilteredUsers = await query.ToListAsync();

    // === BƯỚC 3: TÍNH TOÁN TRẠNG THÁI CHO TẤT CẢ USER TRONG BỘ NHỚ ===
    const int inactiveDaysThreshold = 30;
    foreach (var user in allFilteredUsers)
    {
        // 1. Ưu tiên "Bị cấm"
        if (user.UserStatus.Equals("Bị cấm", StringComparison.OrdinalIgnoreCase))
        {
            // Trạng thái đã đúng, không cần làm gì
            continue;
        }

        // 2. Tính toán "Không hoạt động"
        if (user.LastLoginAt == null || user.LastLoginAt < DateTime.Now.AddDays(-inactiveDaysThreshold))
        {
            user.UserStatus = "Không hoạt động";
        }
        else
        {
            user.UserStatus = "Hoạt động";
        }
    }

    // === BƯỚC 4: ÁP DỤNG BỘ LỌC TRẠNG THÁI TRÊN DANH SÁCH ĐÃ TÍNH TOÁN ===
    IEnumerable<User> usersAfterStatusFilter = allFilteredUsers;
    if (!string.IsNullOrEmpty(statusFilter) && !statusFilter.Equals("all", StringComparison.OrdinalIgnoreCase))
    {
        // Bây giờ, việc lọc này được thực hiện trên C# (LINQ to Objects) nên sẽ chính xác
        usersAfterStatusFilter = allFilteredUsers.Where(u => u.UserStatus.Equals(statusFilter, StringComparison.OrdinalIgnoreCase));
    }

    // === BƯỚC 5: PHÂN TRANG TRÊN KẾT QUẢ CUỐI CÙNG ===
    var totalUsers = usersAfterStatusFilter.Count();
    var totalPages = (totalUsers > 0 && pageSize > 0) ? (int)Math.Ceiling((double)totalUsers / pageSize) : 1;
    if (pageNumber > totalPages) pageNumber = totalPages;
    if (pageNumber < 1) pageNumber = 1;

    var usersForCurrentPage = usersAfterStatusFilter
                                .OrderByDescending(u => u.CreatedAt)
                                .Skip((pageNumber - 1) * pageSize)
                                .Take(pageSize)
                                .ToList();

    // === BƯỚC 6: GỬI DỮ LIỆU VỀ VIEW ===
    ViewBag.Users = usersForCurrentPage; // Chỉ gửi dữ liệu của trang hiện tại
    ViewBag.TotalUsers = totalUsers; // Tổng số sau khi đã lọc tất cả
    ViewBag.PageNumber = pageNumber;
    ViewBag.PageSize = pageSize;
    ViewBag.TotalPages = totalPages;
    ViewBag.SearchTerm = searchTerm;
    ViewBag.RoleFilter = roleFilter;
    ViewBag.StatusFilter = statusFilter;

    return View();
}
    // GET: /Admin/Settings
    public async Task<IActionResult> Settings()
    {
        int adminUserId = GetCurrentAdminUserId();
        var adminUser = await _context.Users.FindAsync(adminUserId);

        if (adminUser == null)
        {
            return NotFound("Không tìm thấy tài khoản admin.");
        }

        Dictionary<string, object?> settings = new Dictionary<string, object?>();

        if (!string.IsNullOrEmpty(adminUser.Bio))
        {
            try
            {
                var parsedJson = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(adminUser.Bio);
                if (parsedJson != null)
                {
                    foreach (var item in parsedJson)
                    {
                        switch (item.Value.ValueKind)
                        {
                            case JsonValueKind.String:
                                settings[item.Key] = item.Value.GetString();
                                break;
                            case JsonValueKind.Number:
                                if (item.Value.TryGetInt32(out int intVal)) settings[item.Key] = intVal;
                                else if (item.Value.TryGetDouble(out double dblVal)) settings[item.Key] = dblVal;
                                else settings[item.Key] = item.Value.GetRawText();
                                break;
                            case JsonValueKind.True:
                                settings[item.Key] = true;
                                break;
                            case JsonValueKind.False:
                                settings[item.Key] = false;
                                break;
                            case JsonValueKind.Null:
                                settings[item.Key] = null;
                                break;
                        }
                    }
                }
            }
            catch (JsonException)
            {
                // ModelState.AddModelError("Bio", "Dữ liệu cài đặt trong Bio không hợp lệ.");
            }
        }

        // Gán giá trị từ JSON hoặc mặc định vào ViewData
        ViewData["SiteName"] = settings.GetValueOrDefault("SiteName", "Tên Trang Web Mặc Định");
        ViewData["SiteDescription"] = settings.GetValueOrDefault("SiteDescription", "Mô tả mặc định.");
        // URL trang web sẽ lấy từ config, không từ Bio nữa
        ViewData["SiteUrl_ReadOnly"] = _configuration["AppSettings:SiteUrl"] ?? "https://chua-cau-hinh.com";

        ViewData["AdminPanelLanguage"] = settings.GetValueOrDefault("AdminPanelLanguage", "vi");
        ViewData["Timezone"] = settings.GetValueOrDefault("Timezone", "Asia/Ho_Chi_Minh");
        ViewData["DateFormat"] = settings.GetValueOrDefault("DateFormat", "dd/MM/yyyy");
        ViewData["PostsPerPage"] = settings.GetValueOrDefault("PostsPerPage", 10);
        ViewData["CommentsPerPage"] = settings.GetValueOrDefault("CommentsPerPage", 10);
        ViewData["DefaultCategoryId"] = settings.GetValueOrDefault("DefaultCategoryId", null);
        ViewData["AutoApproveCommentsRegisteredUsers"] = settings.GetValueOrDefault("AutoApproveCommentsRegisteredUsers", true);
        ViewData["AdminTheme"] = settings.GetValueOrDefault("AdminTheme", "system");
        ViewData["AdminPrimaryColor"] = settings.GetValueOrDefault("AdminPrimaryColor", "#3B82F6");
        ViewData["AdminFontFamily"] = settings.GetValueOrDefault("AdminFontFamily", "Roboto, sans-serif");
        ViewData["NotifyInAppNewComment"] = settings.GetValueOrDefault("NotifyInAppNewComment", true);
        ViewData["NotifyEmailNewUser"] = settings.GetValueOrDefault("NotifyEmailNewUser", false);
        ViewData["RequireCommentApprovalGlobal"] = settings.GetValueOrDefault("RequireCommentApprovalGlobal", true);
        ViewData["RequireEmailVerification"] = settings.GetValueOrDefault("RequireEmailVerification", true);
        ViewData["SystemVersion"] = settings.GetValueOrDefault("SystemVersion", "1.0.0");

        // Không cần truyền ID bài viết chính sách/điều khoản nữa nếu chúng là trang tĩnh
        // ViewBag.Categories = await _context.Categories.ToListAsync();

        return View("~/Views/Admin/Settings.cshtml");
    }

    // POST: /Admin/SaveSettings
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveSettings(IFormCollection form)
    {
        int adminUserId = GetCurrentAdminUserId();
        var adminUser = await _context.Users.FindAsync(adminUserId);

        if (adminUser == null)
        {
            return NotFound("Không tìm thấy tài khoản admin.");
        }

        var settingsToSave = new Dictionary<string, object?>();

        settingsToSave["SiteName"] = form["SiteName"].ToString();
        settingsToSave["SiteDescription"] = form["SiteDescription"].ToString();
        // KHÔNG LƯU SiteUrl từ form, nó được đọc từ config
        // settingsToSave["SiteUrl"] = form["SiteUrl"].ToString(); // Bỏ dòng này

        settingsToSave["AdminPanelLanguage"] = form["AdminPanelLanguage"].ToString();
        settingsToSave["Timezone"] = form["Timezone"].ToString();
        settingsToSave["DateFormat"] = form["DateFormat"].ToString();

        if (int.TryParse(form["PostsPerPage"], out int postsPerPage))
            settingsToSave["PostsPerPage"] = postsPerPage;
        else
            settingsToSave["PostsPerPage"] = 10;

        if (int.TryParse(form["CommentsPerPage"], out int commentsPerPage))
            settingsToSave["CommentsPerPage"] = commentsPerPage;
        else
            settingsToSave["CommentsPerPage"] = 10;

        if (int.TryParse(form["DefaultCategoryId"], out int defaultCategoryId) && defaultCategoryId > 0)
            settingsToSave["DefaultCategoryId"] = defaultCategoryId;
        else
            settingsToSave["DefaultCategoryId"] = null;

        settingsToSave["AutoApproveCommentsRegisteredUsers"] = form.ContainsKey("AutoApproveCommentsRegisteredUsers") &&
            (form["AutoApproveCommentsRegisteredUsers"].ToString().ToLower() == "true" || form["AutoApproveCommentsRegisteredUsers"].ToString().ToLower() == "on");

        settingsToSave["AdminTheme"] = form["AdminTheme"].ToString();
        settingsToSave["AdminPrimaryColor"] = form["AdminPrimaryColor"].ToString();
        settingsToSave["AdminFontFamily"] = form["AdminFontFamily"].ToString();

        settingsToSave["NotifyInAppNewComment"] = form.ContainsKey("NotifyInAppNewComment") && (form["NotifyInAppNewComment"].ToString().ToLower() == "true" || form["NotifyInAppNewComment"].ToString().ToLower() == "on");
        settingsToSave["NotifyEmailNewUser"] = form.ContainsKey("NotifyEmailNewUser") && (form["NotifyEmailNewUser"].ToString().ToLower() == "true" || form["NotifyEmailNewUser"].ToString().ToLower() == "on");

        settingsToSave["RequireCommentApprovalGlobal"] = form.ContainsKey("RequireCommentApprovalGlobal") && (form["RequireCommentApprovalGlobal"].ToString().ToLower() == "true" || form["RequireCommentApprovalGlobal"].ToString().ToLower() == "on");
        settingsToSave["RequireEmailVerification"] = form.ContainsKey("RequireEmailVerification") && (form["RequireEmailVerification"].ToString().ToLower() == "true" || form["RequireEmailVerification"].ToString().ToLower() == "on");

        settingsToSave["SystemVersion"] = form["SystemVersion"].ToString();

        adminUser.Bio = JsonSerializer.Serialize(settingsToSave, new JsonSerializerOptions { WriteIndented = true });

        try
        {
            _context.Users.Update(adminUser);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Cài đặt đã được lưu thành công!";
        }
        catch (DbUpdateException ex)
        {
            TempData["ErrorMessage"] = $"Lỗi khi lưu cài đặt: {ex.Message}";
        }

        return RedirectToAction("Settings");
    }
    // ===== ĐÂY LÀ ACTION ĐỂ XỬ LÝ YÊU CẦU XÓA TỪ AJAX =====
    [HttpPost]
    [ValidateAntiForgeryToken] // Rất quan trọng để khớp với token từ JS
    public async Task<IActionResult> DeleteComment([FromBody] DeleteCommentRequest request)
    {
        // [FromBody] rất quan trọng vì JS gửi dữ liệu dạng JSON
        if (request == null || request.Id <= 0 || string.IsNullOrEmpty(request.Type))
        {
            return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
        }

        try
        {
            if (request.Type == "Review")
            {
                var review = await _context.Reviews.FindAsync(request.Id); // Sửa Reviews thành tên DbSet của bạn
                if (review == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đánh giá để xóa." });
                }
                _context.Reviews.Remove(review);
            }
            else if (request.Type == "PostComment")
            {
                var comment = await _context.PostComments.FindAsync(request.Id); // Sửa PostComments thành tên DbSet của bạn
                if (comment == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy bình luận để xóa." });
                }
                _context.PostComments.Remove(comment);
            }
            else
            {
                return Json(new { success = false, message = "Loại mục không được hỗ trợ." });
            }

            await _context.SaveChangesAsync();
            
            // Trả về kết quả thành công dưới dạng JSON để JavaScript xử lý
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            // Ghi lại lỗi để debug (tùy chọn)
            // _logger.LogError(ex, "Lỗi khi xóa bình luận/đánh giá.");
            return Json(new { success = false, message = "Đã xảy ra lỗi hệ thống." });
        }
    }
}
