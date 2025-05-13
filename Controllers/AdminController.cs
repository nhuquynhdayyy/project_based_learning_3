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
        var users = _context.Users.ToList(); // Example: Fetch users
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
    ViewBag.SelectedTimeRange = timeRange; // Truyền giá trị đã chọn vào ViewBag

    // --- Lấy dữ liệu cho Stats Cards ---
    viewModel.TotalPosts = await _context.Posts
                                        .Where(p => p.CreatedAt >= startDate ) //&& p.Status == PostStatus.Approved
                                        .CountAsync();

    // Số lượng này vẫn có thể hữu ích cho mục đích khác, nhưng không cho biểu đồ phân bố này
    // viewModel.TotalTouristSpots = await _context.TouristSpots
    //                                          .Where(ts => ts.CreatedAt >= startDate)
    //                                          .CountAsync();

    viewModel.PostsInGuidebookCategory = await _context.Posts
                                                .Where(p => p.TypeOfPost == "Cẩm nang" && p.CreatedAt >= startDate )
                                                .CountAsync();
    viewModel.PostsInExperienceCategory = await _context.Posts
                                                 .Where(p => p.TypeOfPost == "Trải nghiệm" && p.CreatedAt >= startDate )
                                                 .CountAsync();
    viewModel.PostsInLocationCategory = await _context.Posts // Đây là bài viết có TypeOfPost == "Địa điểm"
                                                 .Where(p => p.TypeOfPost == "Địa điểm" && p.CreatedAt >= startDate )
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
    // public IActionResult Interactions()
    // {
    //     return View();
    // }
    public async Task<IActionResult> Interactions(string timeRange = "30")
{
    // Convert timeRange to int for easier handling
    int days;
    bool isAllTime = false;
    
    switch (timeRange)
    {
        case "7":
            days = 7;
            break;
        case "-1":
            days = int.MaxValue; // Use a large value for "All time"
            isAllTime = true;
            break;
        case "30":
        default:
            days = 30;
            break;
    }
    
    ViewBag.SelectedDays = isAllTime ? -1 : days;
    
    // Determine the start date based on selected time range
    DateTime startDate = isAllTime 
        ? new DateTime(2000, 1, 1) // Use a very old date for "All time"
        : DateTime.Now.AddDays(-days);
        
    DateTime previousPeriodStart = isAllTime
        ? new DateTime(2000, 1, 1)
        : startDate.AddDays(-days);
    
    // Tính tổng số lượt thích và chia sẻ trong khoảng thời gian đã chọn
    int totalPostFavorites = await _context.PostFavorites
        .Where(f => f.CreatedAt >= startDate)
        .CountAsync();
        
    int totalPostShares = await _context.PostShares
        .Where(s => s.SharedAt >= startDate)
        .CountAsync();
        
    int totalSpotFavorites = await _context.SpotFavorites
        .Where(f => f.CreatedAt >= startDate)
        .CountAsync();
        
    int totalSpotShares = await _context.SpotShares
        .Where(s => s.SharedAt >= startDate)
        .CountAsync();
        
    // Tính tổng số lượt thích và chia sẻ trong khoảng thời gian trước đó để tính tỷ lệ tăng trưởng
    int previousPostFavorites = await _context.PostFavorites
        .Where(f => f.CreatedAt >= previousPeriodStart && f.CreatedAt < startDate)
        .CountAsync();
        
    int previousPostShares = await _context.PostShares
        .Where(s => s.SharedAt >= previousPeriodStart && s.SharedAt < startDate)
        .CountAsync();
        
    int previousSpotFavorites = await _context.SpotFavorites
        .Where(f => f.CreatedAt >= previousPeriodStart && f.CreatedAt < startDate)
        .CountAsync();
        
    int previousSpotShares = await _context.SpotShares
        .Where(s => s.SharedAt >= previousPeriodStart && s.SharedAt < startDate)
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
    
    // Tính số ngày hiển thị trên biểu đồ (tối đa 30 điểm dữ liệu)
    int displayDays = isAllTime ? 30 : days;
    int interval = displayDays <= 30 ? 1 : displayDays / 30;
    
    // Nếu là "Tất cả", lấy dữ liệu theo tháng thay vì theo ngày
    if (isAllTime)
    {
        // Lấy dữ liệu theo tháng cho "Tất cả"
        var currentDate = DateTime.Now;
        for (int i = 0; i < 12; i++) // Hiển thị 12 tháng gần nhất
        {
            var monthStart = new DateTime(currentDate.Year, currentDate.Month, 1).AddMonths(-i);
            var monthEnd = monthStart.AddMonths(1);
            
            dateLabels.Insert(0, monthStart.ToString("MM/yyyy"));
            
            postFavoritesData.Insert(0, await _context.PostFavorites
                .Where(f => f.CreatedAt >= monthStart && f.CreatedAt < monthEnd)
                .CountAsync());
                
            postSharesData.Insert(0, await _context.PostShares
                .Where(s => s.SharedAt >= monthStart && s.SharedAt < monthEnd)
                .CountAsync());
                
            spotFavoritesData.Insert(0, await _context.SpotFavorites
                .Where(f => f.CreatedAt >= monthStart && f.CreatedAt < monthEnd)
                .CountAsync());
                
            spotSharesData.Insert(0, await _context.SpotShares
                .Where(s => s.SharedAt >= monthStart && s.SharedAt < monthEnd)
                .CountAsync());
        }
    }
    else
    {
        // Lấy dữ liệu theo ngày cho 7 hoặc 30 ngày
        for (int i = 0; i < displayDays; i += interval)
        {
            DateTime currentDate = DateTime.Now.AddDays(-(displayDays - i - 1));
            DateTime endDate = currentDate.AddDays(1);
            
            dateLabels.Add(currentDate.ToString("dd/MM"));
            
            postFavoritesData.Add(await _context.PostFavorites
                .Where(f => f.CreatedAt >= currentDate && f.CreatedAt < endDate)
                .CountAsync());
                
            postSharesData.Add(await _context.PostShares
                .Where(s => s.SharedAt >= currentDate && s.SharedAt < endDate)
                .CountAsync());
                
            spotFavoritesData.Add(await _context.SpotFavorites
                .Where(f => f.CreatedAt >= currentDate && f.CreatedAt < endDate)
                .CountAsync());
                
            spotSharesData.Add(await _context.SpotShares
                .Where(s => s.SharedAt >= currentDate && s.SharedAt < endDate)
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
            FavoritesCount = p.PostFavorites.Count(f => f.CreatedAt >= startDate),
            SharesCount = p.Shares.Count(s => s.SharedAt >= startDate)
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
            FavoritesCount = s.Favorites.Count(f => f.CreatedAt >= startDate),
            SharesCount = s.Shares.Count(sh => sh.SharedAt >= startDate)
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
            PostFavoritesCount = u.PostFavorites.Count(f => f.CreatedAt >= startDate),
            PostSharesCount = u.PostShares.Count(s => s.SharedAt >= startDate),
            SpotFavoritesCount = u.SpotFavorites.Count(f => f.CreatedAt >= startDate),
            SpotSharesCount = u.SpotShares.Count(s => s.SharedAt >= startDate)
        })
        .OrderByDescending(u => 
            u.PostFavoritesCount + u.PostSharesCount + u.SpotFavoritesCount + u.SpotSharesCount)
        .Take(5)
        .ToListAsync();
        
    ViewBag.TopUsers = topUsers;
    
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
