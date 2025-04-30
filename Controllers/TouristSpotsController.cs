using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TourismWeb.Models;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace TourismWeb.Controllers
{
    public class TouristSpotsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public TouristSpotsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }
        // GET: TouristSpots
        public async Task<IActionResult> Index(int? categoryId, int? tagId, string sortBy = "default")
        {
            // Bắt đầu với truy vấn cơ bản
            var query = _context.TouristSpots
                .Include(t => t.Category)
                .Include(t => t.Favorites)
                .Include(t => t.Comments)
                .Include(t => t.SpotTags)
                    .ThenInclude(st => st.Tag)
                .AsQueryable();
            
            // Lọc theo danh mục nếu có
            if (categoryId.HasValue)
            {
                query = query.Where(t => t.CategoryId == categoryId.Value);
            }
            
            // Lọc theo tag nếu có
            if (tagId.HasValue)
            {
                query = query.Where(t => t.SpotTags.Any(st => st.TagId == tagId.Value));
            }
            
            // Sắp xếp theo tiêu chí được chọn
            switch (sortBy)
            {
                case "latest":
                    query = query.OrderByDescending(t => t.CreatedAt);
                    break;
                case "mostLiked":
                    query = query.OrderByDescending(t => t.Favorites.Count);
                    break;
                case "popular":
                    query = query.OrderByDescending(t => t.Comments.Count + t.Favorites.Count);
                    break;
                default:
                    query = query.OrderBy(t => t.Name);
                    break;
            }
            
            var touristSpots = await query.ToListAsync();
            
            // Kiểm tra xem người dùng hiện tại đã thích địa điểm nào
            // Giả sử bạn có một phương thức để lấy ID người dùng hiện tại
            // var currentUserId = GetCurrentUserId();
            // if (currentUserId != null)
            // {
            //     foreach (var spot in touristSpots)
            //     {
            //         spot.IsLikedByCurrentUser = spot.Favorites?.Any(f => f.UserId == currentUserId) ?? false;
            //     }
            // }
            var currentUserIdStr = GetCurrentUserId();
            if (int.TryParse(currentUserIdStr, out int currentUserId))
            {
                foreach (var spot in touristSpots)
                {
                    spot.IsLikedByCurrentUser = spot.Favorites?.Any(f => f.UserId == currentUserId) ?? false;
                }
            }

            // Lấy tổng số địa điểm không phân loại
            var allTouristSpotsCount = await _context.TouristSpots.CountAsync(); // Số lượng tất cả địa điểm

            // Truyền tổng số địa điểm vào ViewBag
            ViewBag.AllTouristSpotsCount = allTouristSpotsCount;
            
            // Truyền danh sách danh mục cho sidebar
            ViewBag.Categories = await _context.Categories
                .Include(c => c.TouristSpots)
                .ToListAsync();
            
            // Truyền danh sách địa điểm phổ biến cho sidebar (xử lý ở Controller thay vì View)
            ViewBag.PopularSpots = await _context.TouristSpots
                .Include(t => t.Favorites)
                .OrderByDescending(t => t.Favorites.Count)
                .Take(3)
                .ToListAsync();
            
            // Truyền danh sách bài viết gần đây cho sidebar
            ViewBag.RecentPosts = await _context.Posts
                .OrderByDescending(p => p.CreatedAt)
                .Take(3)
                .ToListAsync();
            
            return View(touristSpots);
        }

        // Phương thức giả định để lấy ID người dùng hiện tại
        private string GetCurrentUserId()
        {
            // Thực hiện logic lấy ID người dùng hiện tại từ hệ thống xác thực của bạn
            // Ví dụ: return User.FindFirstValue(ClaimTypes.NameIdentifier);
            return null; // Thay thế bằng logic thực tế của bạn
        }

        // POST: TouristSpots/ToggleLike/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLike([FromRoute] int id)
        {
            var touristSpot = await _context.TouristSpots
                .Include(t => t.Favorites)
                .FirstOrDefaultAsync(t => t.SpotId == id);
            
            if (touristSpot == null)
            {
                return NotFound();
            }
            
            // Lấy ID người dùng hiện tại
            // var currentUserId = GetCurrentUserId();
            // if (currentUserId == null)
            // {
            //     return Unauthorized();
            // }
            // Lấy ID người dùng hiện tại
            var currentUserIdStr = GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserIdStr) || !int.TryParse(currentUserIdStr, out var currentUserId))
            {
                return Unauthorized(); // Không hợp lệ hoặc không thể chuyển thành int
            }
            
            // Kiểm tra xem người dùng đã thích địa điểm này chưa
            var existingFavorite = touristSpot.Favorites?.FirstOrDefault(f => f.UserId == currentUserId);
            
            if (existingFavorite != null)
            {
                // Nếu đã thích, hủy thích
                _context.SpotFavorites.Remove(existingFavorite);
            }
            else
            {
                // Nếu chưa thích, thêm thích mới
                var newFavorite = new SpotFavorite
                {
                    SpotId = id,
                    UserId = currentUserId,
                    CreatedAt = DateTime.Now
                };
                _context.SpotFavorites.Add(newFavorite);
            }
            
            await _context.SaveChangesAsync();
            
            // Trả về số lượt thích mới
            // var likeCount = touristSpot.Favorites?.Count ?? 0;
            // if (existingFavorite == null)
            // {
            //     likeCount++; // Nếu vừa thêm thích mới
            // }
            // else
            // {
            //     likeCount--; // Nếu vừa hủy thích
            // }
            // Lấy lại số lượt thích thật từ database sau khi cập nhật
            var likeCount = await _context.SpotFavorites.CountAsync(f => f.SpotId == id);
            
            return Json(new { success = true, likeCount = likeCount });
        }

        // GET: TouristSpots/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var touristSpot = await _context.TouristSpots
                .Include(t => t.Category)
                .FirstOrDefaultAsync(m => m.SpotId == id);
            if (touristSpot == null)
            {
                return NotFound();
            }

            return View(touristSpot);
        }

        // GET: TouristSpots/Create
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.CategoryList = new SelectList(_context.Categories, "CategoryId", "Name");
            return View();
        }

        // POST: TouristSpots/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SpotId,Name,Address,CategoryId,Description,CreatedAt")] TouristSpot touristSpot, IFormFile imageFile)
        {
            Console.WriteLine("CategoryId nhận được: " + touristSpot.CategoryId);
            
            if (ModelState.IsValid)
            {
                // Xử lý tải lên hình ảnh nếu có
                if (imageFile != null && imageFile.Length > 0)
                {
                    // Kiểm tra kích thước file (ví dụ: tối đa 5MB)
                    if (imageFile.Length > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("ImageFile", "Kích thước file không được vượt quá 5MB");
                        ViewBag.CategoryList = new SelectList(_context.Categories, "CategoryId", "Name");
                        return View(touristSpot);
                    }

                    // Kiểm tra loại file
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("ImageFile", "Chỉ chấp nhận file ảnh có định dạng: .jpg, .jpeg, .png, .gif");
                        ViewBag.CategoryList = new SelectList(_context.Categories, "CategoryId", "Name");
                        return View(touristSpot);
                    }
                    // Tạo tên file duy nhất để tránh trùng lặp
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    
                    // Đường dẫn lưu file (trong thư mục wwwroot/images)
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    
                    // Đảm bảo thư mục tồn tại
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }
                    
                    string filePath = Path.Combine(uploadsFolder, fileName);
                    
                    // Lưu file vào thư mục
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }
                    
                    // Cập nhật đường dẫn ảnh trong model
                    touristSpot.ImageUrl = "/images/" + fileName;
                }
                else
                {
                    // Nếu không có ảnh được tải lên, sử dụng ảnh mặc định
                    touristSpot.ImageUrl = "/images/default-spotImage.png";
                }
                
                _context.Add(touristSpot);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    Console.WriteLine(error.ErrorMessage);
                }
            }
            
            ViewBag.CategoryList = new SelectList(_context.Categories, "CategoryId", "Name");
            return View(touristSpot);
        }

        // GET: TouristSpots/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var touristSpot = await _context.TouristSpots.FindAsync(id);
            if (touristSpot == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", touristSpot.CategoryId);
            return View(touristSpot);
        }

        // POST: TouristSpots/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // public async Task<IActionResult> Edit(int id, [Bind("SpotId,Name,Address,CategoryId,Description,ImageUrl,CreatedAt")] TouristSpot touristSpot)
        // {
        //     if (id != touristSpot.SpotId)
        //     {
        //         return NotFound();
        //     }

        //     if (ModelState.IsValid)
        //     {
        //         try
        //         {
        //             _context.Update(touristSpot);
        //             await _context.SaveChangesAsync();
        //         }
        //         catch (DbUpdateConcurrencyException)
        //         {
        //             if (!TouristSpotExists(touristSpot.SpotId))
        //             {
        //                 return NotFound();
        //             }
        //             else
        //             {
        //                 throw;
        //             }
        //         }
        //         return RedirectToAction(nameof(Index));
        //     }
        //     ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", touristSpot.CategoryId);
        //     return View(touristSpot);
        // }
        // POST: TouristSpots/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SpotId,Name,Address,CategoryId,Description,ImageUrl,CreatedAt")] TouristSpot touristSpot, IFormFile imageFile)
        {
            if (id != touristSpot.SpotId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Xử lý tải lên hình ảnh mới nếu có
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        // Kiểm tra kích thước file (ví dụ: tối đa 5MB)
                        if (imageFile.Length > 5 * 1024 * 1024)
                        {
                            ModelState.AddModelError("ImageFile", "Kích thước file không được vượt quá 5MB");
                            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", touristSpot.CategoryId);
                            return View(touristSpot);
                        }

                        // Kiểm tra loại file
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                        var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
                        if (!allowedExtensions.Contains(extension))
                        {
                            ModelState.AddModelError("ImageFile", "Chỉ chấp nhận file ảnh có định dạng: .jpg, .jpeg, .png, .gif");
                            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", touristSpot.CategoryId);
                            return View(touristSpot);
                        }
                        
                        // Tạo tên file duy nhất để tránh trùng lặp
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                        
                        // Đường dẫn lưu file (trong thư mục wwwroot/images)
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                        
                        // Đảm bảo thư mục tồn tại
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }
                        
                        string filePath = Path.Combine(uploadsFolder, fileName);
                        
                        // Lưu file vào thư mục
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(fileStream);
                        }
                        
                        // Xóa ảnh cũ nếu không phải ảnh mặc định
                        var oldImagePath = touristSpot.ImageUrl;
                        if (!string.IsNullOrEmpty(oldImagePath) && !oldImagePath.Contains("default-spotImage.png"))
                        {
                            var oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, oldImagePath.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }
                        
                        // Cập nhật đường dẫn ảnh mới trong model
                        touristSpot.ImageUrl = "/images/" + fileName;
                    }
                    
                    _context.Update(touristSpot);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TouristSpotExists(touristSpot.SpotId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", touristSpot.CategoryId);
            return View(touristSpot);
        }

        // GET: TouristSpots/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var touristSpot = await _context.TouristSpots
                .Include(t => t.Category)
                .FirstOrDefaultAsync(m => m.SpotId == id);
            if (touristSpot == null)
            {
                return NotFound();
            }

            return View(touristSpot);
        }

        // POST: TouristSpots/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var touristSpot = await _context.TouristSpots.FindAsync(id);
            if (touristSpot != null)
            {
                _context.TouristSpots.Remove(touristSpot);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TouristSpotExists(int id)
        {
            return _context.TouristSpots.Any(e => e.SpotId == id);
        }
    }
}
