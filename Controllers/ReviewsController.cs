using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TourismWeb.Models;
using System.Security.Claims;

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

        // public ReviewsController(ApplicationDbContext context)
        // {
        //     _context = context;
        // }

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

        // POST: Reviews/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // public async Task<IActionResult> Create([Bind("ReviewId,SpotId,Rating,Comment,ImageUrl,CreatedAt")] Review review)
        // {
        //     if (ModelState.IsValid)
        //     {
        //         var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        //         if (userIdClaim == null)
        //         {                    
        //             return Unauthorized();
        //         }

        //         review.UserId = int.Parse(userIdClaim.Value);
        //         review.CreatedAt = DateTime.Now;

        //         if (string.IsNullOrEmpty(review.ImageUrl))
        //         {
        //             review.ImageUrl = "/images/default-postImage.png";
        //         }
        //         _context.Add(review);
        //         await _context.SaveChangesAsync();
        //         // return RedirectToAction(nameof(Index));
        //         return RedirectToAction("Details", "TouristSpots", new { id = review.SpotId });
        //     }
        //     ViewData["SpotId"] = new SelectList(_context.TouristSpots, "SpotId", "Name", review.SpotId);
        //     return View(review);
        // }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SpotId,Rating,Comment")] Review review, IFormFile imageFile)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized();

            review.UserId = int.Parse(userIdClaim.Value);
            review.CreatedAt = DateTime.Now;

            // Xử lý ảnh tải lên nếu có
            if (imageFile != null && imageFile.Length > 0)
            {
                // Kiểm tra kích thước file (ví dụ: tối đa 5MB)
                if (imageFile.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("ImageFile", "Kích thước file không được vượt quá 5MB");
                    return RedirectToAction("Details", "TouristSpots", new { id = review.SpotId });
                }

                // Kiểm tra loại file
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("ImageFile", "Chỉ chấp nhận các định dạng: .jpg, .jpeg, .png, .gif");
                    return RedirectToAction("Details", "TouristSpots", new { id = review.SpotId });
                }

                // Tạo tên file duy nhất
                string fileName = Guid.NewGuid().ToString() + extension;

                // Thư mục lưu ảnh: wwwroot/images/reviews
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "reviews");

                // Tạo thư mục nếu chưa tồn tại
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Đường dẫn vật lý để lưu
                string filePath = Path.Combine(uploadsFolder, fileName);

                // Lưu file vào hệ thống
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                // Gán đường dẫn vào Review
                review.ImageUrl = "/images/reviews/" + fileName;
            }
            else
            {
                // Nếu không có ảnh, gán ảnh mặc định
                review.ImageUrl = "/images/default-postImage.png";
            }

            // Lưu vào database
            _context.Add(review);
            await _context.SaveChangesAsync();

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
            ViewData["SpotId"] = new SelectList(_context.TouristSpots, "SpotId", "Name", review.SpotId);
            return View(review);
        }

        // POST: Reviews/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ReviewId,SpotId,Rating,Comment,ImageUrl,CreatedAt")] Review review)
        {
            if (id != review.ReviewId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                    if (userIdClaim == null)
                    {
                        return Unauthorized();
                    }

                    var existingReview = await _context.Reviews.FirstOrDefaultAsync(p => p.ReviewId == id);
                    if (existingReview == null)
                    {
                        return NotFound();
                    }

                    // Kiểm tra người dùng có quyền sửa bài viết này không
                    if (existingReview.UserId != int.Parse(userIdClaim.Value))
                    {
                        return Unauthorized();
                    }

                    // Cập nhật từng field cho existingPost
                    existingReview.SpotId = review.SpotId;
                    existingReview.Rating = review.Rating;
                    existingReview.Comment = review.Comment;
                    existingReview.ImageUrl = string.IsNullOrEmpty(review.ImageUrl) 
                        ? existingReview.ImageUrl ?? "/images/default-postImage.png" 
                        : review.ImageUrl;

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
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

            return View(review);
        }

        // POST: Reviews/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review != null)
            {
                _context.Reviews.Remove(review);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReviewExists(int id)
        {
            return _context.Reviews.Any(e => e.ReviewId == id);
        }
    }
}
