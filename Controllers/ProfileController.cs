using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using TourismWeb.Models;

namespace TourismWeb.Controllers
{
    [Authorize] // Đảm bảo người dùng đã đăng nhập
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProfileController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Lấy ID của người dùng đang đăng nhập
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            // Nếu sử dụng Identity, bạn có thể cần chuyển đổi userId từ string sang int
            if (int.TryParse(userId, out int userIdInt))
            {
                // Lấy thông tin người dùng từ database, bao gồm các collection liên quan
                var user = await _context.Users
                    .Include(u => u.Posts)
                    .Include(u => u.Reviews)
                    .Include(u => u.SpotImages)
                    .Include(u => u.SpotFavorites)
                        .ThenInclude(sf => sf.Spot)
                    .Include(u => u.PostFavorites)
                        .ThenInclude(pf => pf.Post)
                            .ThenInclude(p => p.User)
                    .FirstOrDefaultAsync(u => u.UserId == userIdInt);

                if (user != null)
                {
                    return View(user);
                }
            }

            // Nếu không tìm thấy người dùng, chuyển hướng đến trang đăng nhập
            return RedirectToAction("Login", "Account");
        }
    }
}