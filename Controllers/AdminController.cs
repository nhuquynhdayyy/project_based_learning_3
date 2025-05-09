using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourismWeb.Models;
using TourismWeb.Models.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

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
        return View(); // This usually redirects to Dashboard or another default admin page
    }

    public IActionResult ManageUsers()
    {
        var users = _context.Users.ToList(); // Example: Fetch users
        return View(users);
    }

    public async Task<IActionResult> Dashboard(string timeRange = "30")
    {
        var viewModel = new DashboardViewModel();
        DateTime startDate;

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

        viewModel.TotalPosts = await _context.Posts
                                            .Where(p => p.CreatedAt >= startDate)
                                            .CountAsync();
        viewModel.TotalTouristSpots = await _context.TouristSpots
                                             .Where(ts => ts.CreatedAt >= startDate)
                                             .CountAsync();
        viewModel.PostsInGuidebookCategory = await _context.Posts
                                                    .Where(p => p.TypeOfPost == "Cẩm nang" && p.CreatedAt >= startDate)
                                                    .CountAsync();
        viewModel.PostsInExperienceCategory = await _context.Posts
                                                     .Where(p => p.TypeOfPost == "Trải nghiệm" && p.CreatedAt >= startDate)
                                                     .CountAsync();
        viewModel.PostsInLocationCategory = await _context.Posts // Assuming "Địa điểm" is a TypeOfPost
                                                     .Where(p => p.TypeOfPost == "Địa điểm" && p.CreatedAt >= startDate)
                                                     .CountAsync();

        var postsByDay = await _context.Posts
                                .Where(p => p.CreatedAt >= startDate)
                                .GroupBy(p => p.CreatedAt.Date)
                                .Select(g => new { Date = g.Key, Count = g.Count() })
                                .OrderBy(x => x.Date)
                                .ToListAsync();

        viewModel.PostChartLabels = postsByDay.Select(pd => pd.Date.ToString("dd/MM")).ToList();
        viewModel.PostChartData = postsByDay.Select(pd => pd.Count).ToList();

        var postsDistribution = await _context.Posts
                                    .Where(p => p.CreatedAt >= startDate)
                                    .GroupBy(p => p.TypeOfPost)
                                    .Select(g => new { TypeName = g.Key, Count = g.Count() })
                                    .ToListAsync();

        viewModel.DistributionChartLabels = postsDistribution.Select(d => d.TypeName ?? "Không xác định").ToList();
        viewModel.DistributionChartData = postsDistribution.Select(d => d.Count).ToList();

        viewModel.RecentActivities = await _context.Posts
                                            .Include(p => p.User)
                                            .OrderByDescending(p => p.CreatedAt)
                                            .Take(5)
                                            .ToListAsync();

        return View(viewModel);
    }

    public IActionResult Posts()
    {
        // You'd typically fetch and pass posts here
        // var posts = _context.Posts.Include(p => p.User).OrderByDescending(p => p.CreatedAt).ToList();
        // return View(posts);
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

    public IActionResult Users()
    {
        // var users = _context.Users.ToList();
        // return View(users);
        return View();
    }

    public IActionResult Settings()
    {
        return View();
    }

}