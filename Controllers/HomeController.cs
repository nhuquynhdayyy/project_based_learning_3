using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourismWeb.Models;

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
