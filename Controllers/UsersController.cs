using Microsoft.AspNetCore.Mvc;
using TourismWeb.Models;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;

namespace TourismWeb.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Hiển thị form đăng ký
        public IActionResult Register()
        {
            return View();
        }

        // Xử lý đăng ký
        [HttpPost]
        public async Task<IActionResult> Register(User user)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    Console.WriteLine(error.ErrorMessage); // In lỗi ra console
                }
                return View(user);
            }

            // Kiểm tra tài khoản đã tồn tại chưa
            var existingUser = _context.Users.FirstOrDefault(u => u.Email == user.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Email này đã được sử dụng!");
                return View(user);
            }

            // Gán giá trị mặc định nếu chưa có
            user.Role = user.Role ?? "User";  // Mặc định là User
            user.AvatarUrl = string.IsNullOrEmpty(user.AvatarUrl) ? "default-avatar.png" : user.AvatarUrl; // Avatar mặc định
            user.PhoneNumber = string.IsNullOrEmpty(user.PhoneNumber) ? "0000000000" : user.PhoneNumber; // Số điện thoại giả định
            user.TwoFaSecret = user.TwoFaSecret ?? ""; // Nếu không dùng 2FA, để trống
            
            // Mã hóa mật khẩu trước khi lưu (Không dùng salt)
            user.PasswordHash = HashPassword(user.PasswordHash);

            try
            {
                // Thêm user vào database
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Gửi thông báo đăng ký thành công
                ViewBag.SuccessMessage = "Đăng ký thành công! Bạn có thể đăng nhập ngay.";

                // Hiển thị lại form nhưng có nút đăng nhập
                return View();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                return View(user);
            }
        }

        // Mã hóa mật khẩu (Không dùng salt)
        private string HashPassword(string password)
        {
            return Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: new byte[16], // Không dùng salt
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string UsernameOrEmail, string Password)
        {
            // Kiểm tra nếu Session đã sẵn sàng
            if (HttpContext.Session == null)
            {
                return BadRequest("Session chưa được cấu hình. Hãy kiểm tra cấu hình Startup.cs hoặc Program.cs.");
            }

            // Tìm người dùng theo email hoặc username
            var user = _context.Users.FirstOrDefault(u => u.Email == UsernameOrEmail || u.Username == UsernameOrEmail);

            if (user == null)
            {
                ModelState.AddModelError("", "Tài khoản không tồn tại!");
                return View();
            }

            // Hash mật khẩu nhập vào để so sánh với database
            string hashedInputPassword = HashPassword(Password);

            if (user.PasswordHash != hashedInputPassword)
            {
                ModelState.AddModelError("", "Mật khẩu không chính xác!");
                return View();
            }

            // Lưu session
            HttpContext.Session.SetString("UserId", user.UserId.ToString());

            return RedirectToAction("Index", "Home");
        }

        public IActionResult TestSession()
        {
            HttpContext.Session.SetString("TestKey", "Session is working!");
            string value = HttpContext.Session.GetString("TestKey");

            return Content(value ?? "Session is NOT working!");
        }

    }
}
