
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;   
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
                        .ThenInclude(p => p.Spot)
                    .Include(u => u.Reviews)
                       .ThenInclude(r => r.Spot)
                    .Include(u => u.SpotImages)
                       .ThenInclude(si => si.Spot)
                    .Include(u => u.SpotFavorites)
                        .ThenInclude(sf => sf.Spot)
                    .Include(u => u.PostComments)      // Nạp danh sách bình luận của user này
                        .ThenInclude(c => c.Post)
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

        [HttpPost]
public async Task<IActionResult> UploadAvatar(IFormFile avatarFile)
{
    if (avatarFile == null || avatarFile.Length == 0)
    {
        TempData["ErrorMessage"] = "Vui lòng chọn một tệp ảnh.";
        return RedirectToAction("Index");
    }

    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

    if (!int.TryParse(userId, out int userIdInt))
    {
        TempData["ErrorMessage"] = "Phiên người dùng không hợp lệ.";
        return RedirectToAction("Index");
    }

    var user = await _context.Users.FindAsync(userIdInt);
    if (user == null)
    {
        TempData["ErrorMessage"] = "Không tìm thấy người dùng.";
        return RedirectToAction("Index");
    }

    // Kiểm tra định dạng file ảnh
    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
    var extension = Path.GetExtension(avatarFile.FileName).ToLower();

    if (!allowedExtensions.Contains(extension))
    {
        TempData["ErrorMessage"] = "Chỉ chấp nhận các định dạng ảnh JPG, JPEG, PNG, GIF.";
        return RedirectToAction("Index");
    }

    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/avatars");
    Directory.CreateDirectory(uploadsFolder); // Đảm bảo thư mục tồn tại

    var fileName = $"{Guid.NewGuid()}{extension}";
    var filePath = Path.Combine(uploadsFolder, fileName);

    using (var stream = new FileStream(filePath, FileMode.Create))
    {
        await avatarFile.CopyToAsync(stream);
    }

    // Cập nhật avatar URL
    user.AvatarUrl = $"/images/avatars/{fileName}";
    _context.Update(user);
    await _context.SaveChangesAsync();

    TempData["SuccessMessage"] = "Ảnh đại diện đã được cập nhật.";
    return RedirectToAction("Index");
}


        // POST: /Profile/UpdateProfile
        [HttpPost]
        [ValidateAntiForgeryToken] // Rất quan trọng!
        // Action này sẽ nhận các giá trị từ form thông qua các tham số có tên khớp với thuộc tính 'name' của input
        public async Task<IActionResult> UpdateProfile(
            string FullName, // Nhận từ input có name="FullName"
            int BirthDay,    // Nhận từ select có name="BirthDay"
            int BirthMonth,  // Nhận từ select có name="BirthMonth"
            int BirthYear,   // Nhận từ select có name="BirthYear"
            string PhoneNumber // Nhận từ input có name="PhoneNumber" (Cần thêm input này vào View)
            ) 
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userId, out int userIdInt))
            {
                // Có thể trả về lỗi hoặc trang không tìm thấy
                return Unauthorized("Không thể xác định người dùng.");
            }

            // *** BƯỚC BẢO MẬT QUAN TRỌNG: Lấy user gốc từ DB ***
            var userToUpdate = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userIdInt);

            if (userToUpdate == null)
            {
                return NotFound("Không tìm thấy tài khoản người dùng.");
            }

            bool hasError = false;

            // --- Xác thực và chuẩn bị dữ liệu ---

            // 1. Tên đầy đủ (ví dụ: kiểm tra không rỗng)
            if (string.IsNullOrWhiteSpace(FullName))
            {
                ModelState.AddModelError("FullName", "Vui lòng nhập tên đầy đủ.");
                hasError = true;
            }

            // 2. Ngày sinh
            DateTime? dateOfBirth = null;
            try
            {
                // Kiểm tra tính hợp lệ cơ bản của ngày tháng năm
                if (BirthYear > 0 && BirthMonth > 0 && BirthDay > 0 && BirthDay <= DateTime.DaysInMonth(BirthYear, BirthMonth))
                {
                    dateOfBirth = new DateTime(BirthYear, BirthMonth, BirthDay);
                }
                else if (BirthYear != 0 || BirthMonth != 0 || BirthDay != 0) // Nếu người dùng chọn gì đó nhưng không hợp lệ
                {
                    ModelState.AddModelError("DateOfBirth", "Ngày sinh không hợp lệ.");
                    hasError = true;
                }
                 // Nếu tất cả là 0 hoặc giá trị mặc định, coi như người dùng không muốn đặt ngày sinh (cho phép null)
            }
            catch (ArgumentOutOfRangeException) // Bắt lỗi nếu ngày không hợp lệ (vd: 31/02)
            {
                ModelState.AddModelError("DateOfBirth", "Ngày sinh không hợp lệ.");
                hasError = true;
            }

            // 3. Số điện thoại (có thể thêm validation phức tạp hơn nếu cần)
            // Ví dụ: kiểm tra độ dài hoặc định dạng cơ bản
             if (!string.IsNullOrEmpty(PhoneNumber) && PhoneNumber.Length > 20)
             {
                  ModelState.AddModelError("PhoneNumber", "Số điện thoại quá dài.");
                  hasError = true;
             }


            // Nếu có lỗi validation (từ ModelState)
            if (hasError || !ModelState.IsValid) // Kiểm tra cả lỗi tự thêm và lỗi khác (nếu có)
            {
                // *** QUAN TRỌNG: Cần nạp lại các dữ liệu Include cho user để hiển thị lại View đầy đủ ***
                 userToUpdate = await _context.Users
                    .Include(u => u.Posts)
                    .Include(u => u.Reviews)
                    .Include(u => u.SpotImages)
                    .Include(u => u.SpotFavorites).ThenInclude(sf => sf.Spot)
                    .Include(u => u.PostFavorites).ThenInclude(pf => pf.Post).ThenInclude(p => p.User)
                    .FirstOrDefaultAsync(u => u.UserId == userIdInt);

                // Gán lại giá trị người dùng đã nhập vào model trước khi trả về View để họ không phải nhập lại
                userToUpdate!.FullName = FullName; 
                // userToUpdate.DateOfBirth = dateOfBirth; // Cập nhật DOB để hiển thị lại lựa chọn sai (nếu muốn)
                userToUpdate.PhoneNumber = PhoneNumber;

                TempData["ErrorMessage"] = "Cập nhật thất bại, vui lòng kiểm tra lại thông tin.";
                return View("Index", userToUpdate); // Trả về View Index với model User gốc và lỗi trong ModelState
            }

            // --- Nếu không có lỗi, cập nhật các trường được phép vào user gốc ---
            userToUpdate.FullName = FullName;
            userToUpdate.DateOfBirth = dateOfBirth; // Gán giá trị DateTime? đã xử lý
            userToUpdate.PhoneNumber = string.IsNullOrWhiteSpace(PhoneNumber) ? null : PhoneNumber; // Cho phép xóa số điện thoại

            try
            {
                // Không cần gọi _context.Update() vì userToUpdate đang được context theo dõi
                await _context.SaveChangesAsync(); // Lưu thay đổi vào database

                TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
                return RedirectToAction(nameof(Index)); // Chuyển hướng về trang profile
            }
            catch (DbUpdateConcurrencyException)
            {
                ModelState.AddModelError("", "Không thể lưu thay đổi. Dữ liệu có thể đã bị người khác cập nhật. Vui lòng tải lại trang và thử lại.");
            }
            catch (Exception ex) // Bắt các lỗi khác
            {
                Console.WriteLine(ex); 
                ModelState.AddModelError("", "Đã xảy ra lỗi trong quá trình cập nhật thông tin.");
            }

             // Nếu có lỗi khi lưu DB, nạp lại dữ liệu và trả về View với lỗi
            userToUpdate = await _context.Users
                .Include(u => u.Posts).Include(u => u.Reviews).Include(u => u.SpotImages)
                .Include(u => u.SpotFavorites).ThenInclude(sf => sf.Spot)
                .Include(u => u.PostFavorites).ThenInclude(pf => pf.Post).ThenInclude(p => p.User)
                .FirstOrDefaultAsync(u => u.UserId == userIdInt);

            // Gán lại giá trị người dùng đã nhập
            userToUpdate!.FullName = FullName;
            userToUpdate.PhoneNumber = PhoneNumber;

            return View("Index", userToUpdate);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePassword(string CurrentPassword, string NewPassword, string ConfirmPassword)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userId, out int userIdInt))
            {
                return Unauthorized("Không xác định được người dùng.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userIdInt);
            if (user == null)
            {
                return NotFound("Không tìm thấy người dùng.");
            }

            // --- Validation ---
            bool hasError = false;
            if (string.IsNullOrWhiteSpace(CurrentPassword))
            {
                ModelState.AddModelError("CurrentPassword", "Vui lòng nhập mật khẩu hiện tại.");
                hasError = true;
            }

            if (string.IsNullOrWhiteSpace(NewPassword))
            {
                ModelState.AddModelError("NewPassword", "Vui lòng nhập mật khẩu mới.");
                hasError = true;
            }

            if (NewPassword != ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Mật khẩu xác nhận không khớp.");
                hasError = true;
            }

            // So sánh mật khẩu hiện tại trực tiếp (không mã hóa)
            if (!hasError && user.Password != CurrentPassword)
            {
                ModelState.AddModelError("CurrentPassword", "Mật khẩu hiện tại không đúng.");
                hasError = true;
            }

            if (hasError)
            {
                TempData["ErrorMessage"] = "Đổi mật khẩu thất bại, vui lòng kiểm tra lại thông tin nhập vào.";
                ViewData["ActiveTab"] = "security";

                user = await _context.Users
                    .Include(u => u.Posts).Include(u => u.Reviews).Include(u => u.SpotImages)
                    .Include(u => u.SpotFavorites).ThenInclude(sf => sf.Spot)
                    .Include(u => u.PostFavorites).ThenInclude(pf => pf.Post).ThenInclude(p => p.User)
                    .FirstOrDefaultAsync(u => u.UserId == userIdInt);

                return View("Index", user);
            }

            // Cập nhật mật khẩu đơn giản
            user.Password = NewPassword;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã đổi mật khẩu thành công.";
            return RedirectToAction(nameof(Index));
        }

        
        // Action mới để xử lý cập nhật Bio qua AJAX
    [HttpPost]
    [ValidateAntiForgeryToken] // Kiểm tra token
    public async Task<IActionResult> UpdateBio([FromBody] UpdateBioRequestModel model) // Nhận dữ liệu từ body AJAX
    {
        if (model == null) {
             return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ." });
        }

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdString, out int userId))
        {
            // Thông thường [Authorize] đã xử lý, nhưng kiểm tra lại cho chắc
            return Unauthorized(new { success = false, message = "Người dùng không hợp lệ." });
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound(new { success = false, message = "Không tìm thấy người dùng." });
        }

        try
        {
            // Cập nhật bio, trim khoảng trắng thừa
            user.Bio = model.Bio?.Trim();

            _context.Users.Update(user); // Đánh dấu là đã thay đổi
            await _context.SaveChangesAsync(); // Lưu vào DB

            // Trả về kết quả thành công cùng với bio mới (đã trim)
            return Ok(new { success = true, newBio = user.Bio });
        }
        catch (DbUpdateException ex) // Bắt lỗi cụ thể từ DB nếu có
        {
            // Log lỗi ex
            return StatusCode(500, new { success = false, message = "Lỗi khi lưu dữ liệu. Vui lòng thử lại." });
        }
        catch (Exception ex) // Bắt lỗi chung khác
        {
             // Log lỗi ex
             return StatusCode(500, new { success = false, message = "Đã xảy ra lỗi không mong muốn." });
        }
    }

    }
}

// Model đơn giản để nhận dữ liệu từ AJAX
public class UpdateBioRequestModel
{
    public string? Bio { get; set; }
}