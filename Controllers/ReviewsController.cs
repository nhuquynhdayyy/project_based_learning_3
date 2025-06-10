using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TourismWeb.Models; 
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting; 
using System.IO; 

namespace TourismWeb.Controllers
{
    public class ReviewsController : Controller
    {
        private readonly ApplicationDbContext _context; 
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
        public IActionResult Create()
        {
            ViewData["SpotId"] = new SelectList(_context.TouristSpots, "SpotId", "Name");
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SpotId,Rating,Comment")] Review review, IFormFile imageFile)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized(); 

            review.UserId = int.Parse(userIdClaim.Value);
            review.CreatedAt = DateTime.Now;

            if (imageFile != null && imageFile.Length > 0)
            {
                if (imageFile.Length > 5 * 1024 * 1024) // 5MB
                {
                    TempData["Error"] = "Kích thước file không được vượt quá 5MB"; 
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
            _context.Add(review);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Đăng đánh giá thành công!";
            return RedirectToAction("Details", "TouristSpots", new { id = review.SpotId });
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
                return Forbid(); 
            }
            ViewData["SpotId"] = new SelectList(_context.TouristSpots, "SpotId", "Name", review.SpotId);
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
                return Forbid(); 
            }

            review.UserId = existingReview.UserId;
            review.CreatedAt = existingReview.CreatedAt;


            if (imageFile != null && imageFile.Length > 0)
            {
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

                if (ModelState.IsValid) 
                {
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
            else if (string.IsNullOrEmpty(review.ImageUrl))
            {
                review.ImageUrl = existingReview.ImageUrl; 
            }


            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(review); 
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
            if (userIdClaim == null || review.UserId != int.Parse(userIdClaim.Value)) 
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
            if (userIdClaim == null || review.UserId != int.Parse(userIdClaim.Value)) 
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
            return RedirectToAction("Details", "TouristSpots", new { id = review.SpotId }); 
        }

        private bool ReviewExists(int id)
        {
            return _context.Reviews.Any(e => e.ReviewId == id);
        }


        [HttpGet("Reviews/GetSpotReviewsData")] 
        public async Task<IActionResult> GetSpotReviewsData(int spotId, int page = 1, int pageSize = 3, string sortBy = "newest", string filterBy = "all")
        {
            var spotExists = await _context.TouristSpots.AnyAsync(s => s.SpotId == spotId);
            if (!spotExists)
            {
                return NotFound(new { message = "Địa điểm không tồn tại." });
            }

            var query = _context.Reviews
                                .AsNoTracking() 
                                .Include(r => r.User) 
                                .Where(r => r.SpotId == spotId);

            // Filtering
            if (filterBy != "all" && !string.IsNullOrEmpty(filterBy))
            {
                if (filterBy == "with-photos")
                {
                    query = query.Where(r => !string.IsNullOrEmpty(r.ImageUrl) && r.ImageUrl != "/images/default-review.png");
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
                                .Select(r => new 
                                {
                                    r.ReviewId,
                                    r.Comment,
                                    r.Rating,
                                    r.CreatedAt,
                                    ImageUrl = r.ImageUrl, 
                                    UserId = r.User != null ? r.User.UserId : (int?)null, 
                                    UserFullName = r.User != null ? r.User.FullName : "Người dùng ẩn danh",
                                    UserAvatarUrl = r.User != null ? (r.User.AvatarUrl ?? "/images/default-avatar.png") : "/images/default-avatar.png",
                                    LikeCount = 0 
                                })
                                .ToListAsync();

            return Ok(new { reviews = reviewsData, totalPages, currentPage = page, totalItems });
        }
    }
}