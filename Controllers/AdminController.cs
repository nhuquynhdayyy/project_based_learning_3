using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourismWeb.Models;
using TourismWeb.Models.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic; // Required for List

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

        // Optionally add TouristSpots as a separate category in the distribution chart
        // if (viewModel.TotalTouristSpots > 0 && startDate == DateTime.MinValue) // Only for "all time" perhaps
        // {
        //     viewModel.DistributionChartLabels.Add("Địa điểm (Spots)");
        //     viewModel.DistributionChartData.Add(viewModel.TotalTouristSpots);
        // }

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

    // AdminController.cs
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
        // var users = _context.Users.ToList();
        // return View(users);
        return View();
    }
    public IActionResult Settings()
    {
        return View();
    }
}