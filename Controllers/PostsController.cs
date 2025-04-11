using Microsoft.AspNetCore.Mvc;
using TourismWeb.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore; 
using Microsoft.AspNetCore.Http; 
namespace TourismWeb.Controllers
{
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PostsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Hiển thị danh sách bài viết
        public IActionResult Index()
        {
            var posts = _context.Posts.ToList();
            return View(posts);
        }

        // // Hiển thị form tạo bài viết
        // public IActionResult Create()
        // {
        //     return View();
        // }
        // Hiển thị form tạo bài viết + truyền danh sách địa điểm
        public IActionResult Create()
        {
            ViewBag.Spots = _context.TouristSpots.ToList(); // dùng để tạo dropdown SpotId
            return View();
        }
        // Xử lý tạo bài viết
        [HttpPost]
        public async Task<IActionResult> Create(Post post)
        {
            ViewBag.Spots = _context.TouristSpots.ToList(); // Nếu có lỗi thì load lại
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    Console.WriteLine(error.ErrorMessage); // In lỗi ra console
                }
                return View(post);
            }

            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                ModelState.AddModelError("", "Bạn cần đăng nhập để đăng bài!");
                return View(post);
            }
            post.UserId = int.Parse(userId);
            post.CreatedAt = DateTime.Now;
            ModelState.Remove("User");
            if (!ModelState.IsValid)
            {
                return View(post);
            }


            Console.WriteLine($"DEBUG: Title = {post.Title}, Content = {post.Content}, UserId = {post.UserId}");

            try
            {
                _context.Posts.Add(post);
                await _context.SaveChangesAsync();
                Console.WriteLine("DEBUG: Bài viết đã được lưu thành công.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LỖI: {ex.Message}");
                ModelState.AddModelError("", "Có lỗi xảy ra khi lưu bài viết: " + ex.Message);
                return View(post);
            }

            return RedirectToAction("Index","Home");
        }
    }
}
