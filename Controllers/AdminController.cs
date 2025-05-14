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
