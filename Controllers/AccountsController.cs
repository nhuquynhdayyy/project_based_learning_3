using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using TourismWeb.Models;
using System.Security.Cryptography;
using System.Text;

namespace TourismWeb.Controllers
{
    public class AccountsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Accounts/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: Accounts/Register
        [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Register(User model, string ConfirmPassword)
{
    if (ModelState.IsValid)
    {
        if (model.Password != ConfirmPassword)
        {
            ViewBag.ErrorMessage = "Mật khẩu xác nhận không khớp!";
            return View(model);
        }

        // Check if Username or Email already exists
        if (_context.Users.Any(u => u.Username == model.Username || u.Email == model.Email))
        {
            ViewBag.ErrorMessage = "Tên đăng nhập hoặc Email đã tồn tại!";
            return View(model);
        }

        model.CreatedAt = DateTime.Now;
        model.Role = "User"; // mặc định user thường

        try
        {
            _context.Users.Add(model);
            await _context.SaveChangesAsync();

            ViewBag.SuccessMessage = "Đăng ký thành công!";
            return View();
        }
        catch (Exception ex)
        {
            ViewBag.ErrorMessage = "Lỗi lưu dữ liệu: " + ex.Message;
            return View(model);
        }
    }

    // Nếu ModelState không valid, xuất lỗi chi tiết
    var errors = ModelState.Values.SelectMany(v => v.Errors)
                                   .Select(e => e.ErrorMessage)
                                   .ToList();
    ViewBag.ErrorMessage = string.Join("<br>", errors);

    return View(model);
}


        // GET: Accounts/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: Accounts/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string UsernameOrEmail, string Password, bool RememberMe)
        {
            if (string.IsNullOrEmpty(UsernameOrEmail) || string.IsNullOrEmpty(Password))
            {
                ViewBag.ErrorMessage = "Vui lòng nhập đầy đủ thông tin!";
                return View();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => (u.Username == UsernameOrEmail || u.Email == UsernameOrEmail) && u.Password == Password);

            if (user == null)
            {
                ViewBag.ErrorMessage = "Tên đăng nhập, email hoặc mật khẩu không chính xác!";
                return View();
            }

            // Update last login time
            user.LastLoginAt = DateTime.Now;
            await _context.SaveChangesAsync();

            // Create authentication cookie
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = RememberMe
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                                          new ClaimsPrincipal(claimsIdentity),
                                          authProperties);

            return RedirectToAction("Index", "Home");
        }

        // GET: Accounts/Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Accounts");
        }
    }
}
