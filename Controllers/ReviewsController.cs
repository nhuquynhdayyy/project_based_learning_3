using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TourismWeb.Models; // Đảm bảo namespace này đúng với Model của bạn
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting; // Thêm using này
using System.IO; // Thêm using này

namespace TourismWeb.Controllers
{
    // Không cần thêm [ApiController] hay [Route("api/[controller]")] ở đây
    // nếu bạn muốn controller này vẫn phục vụ các View MVC bình thường.
    // Chúng ta sẽ đặt route cụ thể trên Action API.
    public class ReviewsController : Controller
    {
        private readonly ApplicationDbContext _context; // Đảm bảo ApplicationDbContext là DbContext của bạn
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ReviewsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Reviews
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Reviews.Include(r => r.Spot).Include(r => r.User);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Reviews/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var review = await _context.Reviews
                .Include(r => r.Spot)
                .Include(r => r.User)
                .FirstOrDefaultAsync(m => m.ReviewId == id);
            if (review == null)
            {
                return NotFound();
            }

            return View(review);
        }

        // GET: Reviews/Create
        // Action này có thể vẫn giữ nguyên để phục vụ view tạo Review nếu cần (mặc dù form hiện tại là AJAX)
        public IActionResult Create()
        {
            ViewData["SpotId"] = new SelectList(_context.TouristSpots, "SpotId", "Name");
            // Thêm UserId vào ViewData nếu cần cho form (không bắt buộc nếu form xử lý UserId từ Claims)
            // var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            // if (userIdClaim != null) ViewData["UserId"] = int.Parse(userIdClaim.Value);
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SpotId,Rating,Comment")] Review review, IFormFile imageFile)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized(); // Hoặc RedirectToAction("Login", "Accounts");

            review.UserId = int.Parse(userIdClaim.Value);
            review.CreatedAt = DateTime.Now;

            if (imageFile != null && imageFile.Length > 0)
            {
                if (imageFile.Length > 5 * 1024 * 1024) // 5MB
                {
                    TempData["Error"] = "Kích thước file không được vượt quá 5MB"; // Sử dụng TempData để hiển thị lỗi trên trang redirect
                    return RedirectToAction("Details", "TouristSpots", new { id = review.SpotId });
                }

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
                if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
                {
                    TempData["Error"] = "Chỉ chấp nhận các định dạng ảnh: .jpg, .jpeg, .png, .gif";
                    return RedirectToAction("Details", "TouristSpots", new { id = review.SpotId });
                }

                string fileName = Guid.NewGuid().ToString() + extension;
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "reviews");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                string filePath = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }
                review.ImageUrl = "/images/reviews/" + fileName;
            }
            // Không cần gán ảnh mặc định ở đây nếu logic trên đã xử lý
            // else
            // {
            //     review.ImageUrl = "/images/default-postImage.png"; // Ảnh mặc định nếu không có file tải lên
            // }

            // Chỉ thêm ModelState.IsValid nếu bạn có các validation rules khác ngoài Bind
            // if (ModelState.IsValid) // Nếu có các data annotations khác trên Review model
            // {
            _context.Add(review);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Đăng đánh giá thành công!";
            return RedirectToAction("Details", "TouristSpots", new { id = review.SpotId });
            // }

            // Nếu ModelState không hợp lệ (ví dụ, nếu bạn thêm các DataAnnotations cho Rating, Comment)
            // Cần chuẩn bị lại dữ liệu cho view và quay lại form
            // ViewData["SpotId"] = new SelectList(_context.TouristSpots, "SpotId", "Name", review.SpotId);
            // TempData["Error"] = "Có lỗi xảy ra, vui lòng kiểm tra lại thông tin.";
            // return RedirectToAction("Details", "TouristSpots", new { id = review.SpotId }); // Hoặc return View(review) nếu bạn có view riêng cho Create này
        }


        // GET: Reviews/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
            {
                return NotFound();
            }
            // Kiểm tra quyền sở hữu
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || review.UserId != int.Parse(userIdClaim.Value))
            {
                return Forbid(); // Hoặc Unauthorized()
            }
            ViewData["SpotId"] = new SelectList(_context.TouristSpots, "SpotId", "Name", review.SpotId);
            // ViewData["UserId"] = review.UserId; // User không nên được thay đổi khi edit
            return View(review);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ReviewId,SpotId,Rating,Comment,ImageUrl")] Review review, IFormFile imageFile) // Bỏ CreatedAt khỏi Bind
        {
            if (id != review.ReviewId)
            {
                return NotFound();
            }

            var existingReview = await _context.Reviews.AsNoTracking().FirstOrDefaultAsync(r => r.ReviewId == id);
            if (existingReview == null)
            {
                return NotFound();
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || existingReview.UserId != int.Parse(userIdClaim.Value))
            {
                return Forbid(); // Người dùng không có quyền sửa
            }

            // Giữ lại CreatedAt và UserId gốc
            review.UserId = existingReview.UserId;
            review.CreatedAt = existingReview.CreatedAt;


            if (imageFile != null && imageFile.Length > 0)
            {
                // Xử lý tương tự như Create action
                if (imageFile.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("ImageFile", "Kích thước file không được vượt quá 5MB");
                }
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
                if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("ImageFile", "Chỉ chấp nhận các định dạng ảnh: .jpg, .jpeg, .png, .gif");
                }

                if (ModelState.IsValid) // Kiểm tra ModelState sau khi xử lý file
                {
                    // Xóa ảnh cũ nếu có và không phải là ảnh mặc định
                    if (!string.IsNullOrEmpty(existingReview.ImageUrl) && !existingReview.ImageUrl.Contains("default-"))
                    {
                        var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, existingReview.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    string fileName = Guid.NewGuid().ToString() + extension;
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "reviews");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                    string filePath = Path.Combine(uploadsFolder, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }
                    review.ImageUrl = "/images/reviews/" + fileName;
                }
            }
            else if (string.IsNullOrEmpty(review.ImageUrl)) // Nếu người dùng xóa đường dẫn ảnh hiện tại mà không tải ảnh mới
            {
                review.ImageUrl = existingReview.ImageUrl; // Giữ lại ảnh cũ, hoặc có thể set ảnh default
            }
            // Nếu review.ImageUrl đã có giá trị từ form (người dùng không thay đổi ảnh), nó sẽ được giữ nguyên


            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(review); // Cập nhật toàn bộ object review đã được điều chỉnh
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReviewExists(review.ReviewId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Details", "TouristSpots", new { id = review.SpotId });
            }
            ViewData["SpotId"] = new SelectList(_context.TouristSpots, "SpotId", "Name", review.SpotId);
            return View(review);
        }

        // GET: Reviews/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var review = await _context.Reviews
                .Include(r => r.Spot)
                .Include(r => r.User)
                .FirstOrDefaultAsync(m => m.ReviewId == id);
            if (review == null)
            {
                return NotFound();
            }
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || review.UserId != int.Parse(userIdClaim.Value)) // Và có thể thêm quyền admin
            {
                return Forbid();
            }

            return View(review);
        }

        // POST: Reviews/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
            {
                return NotFound();
            }
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || review.UserId != int.Parse(userIdClaim.Value)) // Và có thể thêm quyền admin
            {
                return Forbid();
            }

            // Xóa ảnh liên quan nếu không phải ảnh mặc định
            if (!string.IsNullOrEmpty(review.ImageUrl) && !review.ImageUrl.Contains("default-"))
            {
                var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, review.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "TouristSpots", new { id = review.SpotId }); // Chuyển hướng về trang chi tiết địa điểm
        }

        private bool ReviewExists(int id)
        {
            return _context.Reviews.Any(e => e.ReviewId == id);
        }


        // === PHẦN THÊM MỚI CHO API ===
        [HttpGet("Reviews/GetSpotReviewsData")] // URL sẽ là /Reviews/GetSpotReviewsData?spotId=...
        public async Task<IActionResult> GetSpotReviewsData(int spotId, int page = 1, int pageSize = 3, string sortBy = "newest", string filterBy = "all")
        {
            // Kiểm tra xem spotId có hợp lệ không (tùy chọn)
            var spotExists = await _context.TouristSpots.AnyAsync(s => s.SpotId == spotId);
            if (!spotExists)
            {
                return NotFound(new { message = "Địa điểm không tồn tại." });
            }

            var query = _context.Reviews
                                .AsNoTracking() // Quan trọng cho các query chỉ đọc
                                .Include(r => r.User) // Chỉ include User nếu bạn cần thông tin User
                                .Where(r => r.SpotId == spotId);

            // Filtering
            if (filterBy != "all" && !string.IsNullOrEmpty(filterBy))
            {
                if (filterBy == "with-photos")
                {
                    query = query.Where(r => !string.IsNullOrEmpty(r.ImageUrl) && r.ImageUrl != "/images/default-postImage.png");
                }
                else if (int.TryParse(filterBy, out int ratingFilter))
                {
                    if (ratingFilter >= 1 && ratingFilter <= 5)
                    {
                        query = query.Where(r => r.Rating == ratingFilter);
                    }
                }
            }

            // Sorting
            switch (sortBy)
            {
                case "oldest":
                    query = query.OrderBy(r => r.CreatedAt);
                    break;
                case "highest":
                    query = query.OrderByDescending(r => r.Rating).ThenByDescending(r => r.CreatedAt);
                    break;
                case "lowest":
                    query = query.OrderBy(r => r.Rating).ThenByDescending(r => r.CreatedAt);
                    break;
                case "newest":
                default:
                    query = query.OrderByDescending(r => r.CreatedAt);
                    break;
            }

            var totalItems = await query.CountAsync();
            var totalPages = (int)System.Math.Ceiling((double)totalItems / pageSize);
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;


            var reviewsData = await query
                                .Skip((page - 1) * pageSize)
                                .Take(pageSize)
                                .Select(r => new // Tạo một DTO (Data Transfer Object) để chỉ trả về những gì cần thiết
                                {
                                    r.ReviewId,
                                    r.Comment,
                                    r.Rating,
                                    r.CreatedAt,
                                    ImageUrl = r.ImageUrl, // Giữ nguyên ImageUrl
                                    UserId = r.User != null ? r.User.UserId : (int?)null, // Kiểm tra User null
                                    UserFullName = r.User != null ? r.User.FullName : "Người dùng ẩn danh",
                                    UserAvatarUrl = r.User != null ? (r.User.AvatarUrl ?? "/images/default-avatar.png") : "/images/default-avatar.png",
                                    LikeCount = 0 // Tạm thời để 0, bạn cần logic để đếm like
                                    // LikeCount = r.Likes.Count() // Nếu bạn có collection Likes
                                })
                                .ToListAsync();

            return Ok(new { reviews = reviewsData, totalPages, currentPage = page, totalItems });
        }
        // === KẾT THÚC PHẦN THÊM MỚI CHO API ===
    }
}