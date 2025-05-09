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
using System.Security.Claims;
using System.Collections.Generic; // Cho List

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
                // .Include(t => t.Comments)
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
                    // query = query.OrderByDescending(t => t.Comments.Count + t.Favorites.Count);
                    query = query.OrderByDescending(t => t.Favorites.Count);
                    break;
                default:
                    query = query.OrderBy(t => t.Name);
                    break;
            }
            
            var touristSpots = await query.ToListAsync();
            
            // var currentUserIdStr = GetCurrentUserId();
            // if (int.TryParse(currentUserIdStr, out int currentUserId))
            // {
            //     foreach (var spot in touristSpots)
            //     {
            //         spot.IsLikedByCurrentUser = spot.Favorites?.Any(f => f.UserId == currentUserId) ?? false;
            //     }
            // }
            // Lấy ID người dùng hiện tại
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        int currentUserId = 0; // Mặc định là 0 nếu người dùng chưa đăng nhập hoặc không tìm thấy claim
        if (userIdClaim != null)
        {
            int.TryParse(userIdClaim.Value, out currentUserId);
        }

        // Kiểm tra xem người dùng hiện tại đã thích địa điểm nào (tối ưu hóa)
        if (currentUserId > 0)
        {
            // Lấy danh sách ID của các địa điểm mà người dùng hiện tại đã yêu thích
            var userFavoriteSpotIds = await _context.SpotFavorites
                                              .Where(f => f.UserId == currentUserId)
                                              .Select(f => f.SpotId)
                                              .Distinct() // Đảm bảo không có ID trùng lặp
                                              .ToListAsync();

            foreach (var spot in touristSpots)
            {
                spot.IsLikedByCurrentUser = userFavoriteSpotIds.Contains(spot.SpotId);
            }
        }
        else
        {
            // Nếu người dùng chưa đăng nhập, tất cả các địa điểm đều chưa được thích bởi họ
            foreach (var spot in touristSpots)
            {
                spot.IsLikedByCurrentUser = false;
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

        // Action mới để xử lý AJAX Toggle Favorite
    [HttpPost]
    // [ValidateAntiForgeryToken] // Quan trọng nếu bạn gửi token từ client
    public async Task<IActionResult> ToggleFavoriteSpot(int spotId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            return Json(new { success = false, message = "Vui lòng đăng nhập để thực hiện thao tác này.", aniauth = true });
        }

        if (!int.TryParse(userIdClaim.Value, out int userId))
        {
            return Json(new { success = false, message = "Lỗi xác thực người dùng." });
        }

        var existingFavorite = await _context.SpotFavorites
                                        .FirstOrDefaultAsync(f => f.SpotId == spotId && f.UserId == userId);

        bool favorited;
        if (existingFavorite != null)
        {
            // Đã yêu thích -> Bỏ yêu thích
            _context.SpotFavorites.Remove(existingFavorite);
            favorited = false;
        }
        else
        {
            // Chưa yêu thích -> Thêm yêu thích
            _context.SpotFavorites.Add(new SpotFavorite { SpotId = spotId, UserId = userId, CreatedAt = DateTime.Now });
            favorited = true;
        }

        await _context.SaveChangesAsync();

        // Lấy lại số lượt thích mới
        int newLikeCount = await _context.SpotFavorites.CountAsync(f => f.SpotId == spotId);

        return Json(new { success = true, favorited = favorited, likeCount = newLikeCount });
    }

        // Phương thức giả định để lấy ID người dùng hiện tại
        private string GetCurrentUserId()
        {
            // Thực hiện logic lấy ID người dùng hiện tại từ hệ thống xác thực của bạn
            // Ví dụ: return User.FindFirstValue(ClaimTypes.NameIdentifier);
            return null; // Thay thế bằng logic thực tế của bạn
        }

        // POST: TouristSpots/ToggleLike/5
        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // [Route("/SpotFavorites/ToggleFavorite/{id}")]
        // public async Task<IActionResult> ToggleFavorite([FromRoute] int id)
        // {
        //     var touristSpot = await _context.TouristSpots
        //         .Include(t => t.Favorites)
        //         .FirstOrDefaultAsync(t => t.SpotId == id);

        //     if (touristSpot == null)
        //     {
        //         return NotFound();
        //     }

        //     var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        //     if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var currentUserId))
        //     {
        //         return Unauthorized();
        //     }

        //     var existingFavorite = touristSpot.Favorites?.FirstOrDefault(f => f.UserId == currentUserId);

        //     if (existingFavorite != null)
        //     {
        //         _context.SpotFavorites.Remove(existingFavorite);
        //     }
        //     else
        //     {
        //         _context.SpotFavorites.Add(new SpotFavorite
        //         {
        //             SpotId = id,
        //             UserId = currentUserId,
        //             CreatedAt = DateTime.Now
        //         });
        //     }

        //     await _context.SaveChangesAsync();

        //     var likeCount = await _context.SpotFavorites.CountAsync(f => f.SpotId == id);

        //     return Json(new
        //     {
        //         success = true,
        //         favorited = existingFavorite == null,
        //         likeCount = likeCount
        //     });
        // }


        // GET: TouristSpots/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var touristSpot = await _context.TouristSpots
                .Include(t => t.Category)
                .Include(t => t.Favorites)
                .Include(s => s.Reviews)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(m => m.SpotId == id);
            if (touristSpot == null)
            {
                return NotFound();
            }

            // Tính toán điểm trung bình
            var averageRating = touristSpot.Reviews.Any() 
                                ? touristSpot.Reviews.Average(r => r.Rating) 
                                : 0;

            // Thêm thông tin điểm trung bình vào model
            // ViewBag.AverageRating = averageRating;

            // Tính số lượng đánh giá cho mỗi sao
            var ratingCounts = new int[5]; // Để đếm số lượng mỗi sao (1 sao đến 5 sao)

            foreach (var review in touristSpot.Reviews)
            {
                int ratingIndex = (int)review.Rating - 1; // Đánh giá từ 1 đến 5, do đó trừ 1
                if (ratingIndex >= 0 && ratingIndex < 5)
                {
                    ratingCounts[ratingIndex]++;
                }
            }

            // Tổng số lượng đánh giá
            var totalReviews = touristSpot.Reviews.Count;

            // Tính tỷ lệ phần trăm cho mỗi sao
            var ratingPercentages = ratingCounts.Select(count => (totalReviews > 0 ? (double)count / totalReviews * 100 : 0)).ToArray();

            // Thêm thông tin vào ViewBag để hiển thị
            ViewBag.AverageRating = averageRating;
            ViewBag.RatingCounts = ratingCounts;
            ViewBag.RatingPercentages = ratingPercentages;
            ViewBag.TotalReviews = totalReviews;

            // Lấy 3 review đầu tiên để hiển thị ban đầu (nếu muốn)
        // Bạn có thể bỏ qua phần này và để JavaScript tải tất cả khi trang load
        // Hoặc bạn có thể gọi action GetFilteredReviews ngay từ đầu với tham số mặc định
        var initialReviews = await GetReviewsQuery(id.Value, "newest", "all", false)
                                    .Take(3)
                                    .ToListAsync();
        touristSpot.Reviews = initialReviews; // Gán lại cho Model để view ban đầu có dữ liệu
            return View(touristSpot);
        }

        // Action mới để xử lý AJAX request cho việc lọc reviews
        // GET: /TouristSpots/GetFilteredReviews
        [HttpGet]
        public async Task<IActionResult> GetFilteredReviews(int spotId, int page = 1, int pageSize = 3, string sortBy = "newest", string filterRating = "all", bool withPhotos = false)
        {
            var query = GetReviewsQuery(spotId, sortBy, filterRating, withPhotos);

            var totalReviews = await query.CountAsync();
            var reviews = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalReviewsForPagination = totalReviews; // Tổng số review sau khi lọc, cho logic nút "Xem thêm"

            // Nếu không còn review nào ở trang này và page > 1 (nghĩa là đang load more mà hết)
            // thì không cần trả về nút "Xem thêm"
            if (!reviews.Any() && page > 1) {
                return Content(""); // Trả về chuỗi rỗng
            }


            return PartialView("_ReviewListPartial", reviews);
        }

        // Hàm helper để xây dựng query cho reviews
        private IQueryable<Review> GetReviewsQuery(int spotId, string sortBy, string filterRating, bool withPhotos)
        {
            var query = _context.Reviews
                                .Include(r => r.User) // Cần User để hiển thị thông tin người đánh giá
                                .Where(r => r.SpotId == spotId);

            // Áp dụng bộ lọc theo rating
            if (filterRating != "all" && int.TryParse(filterRating, out int rating))
            {
                query = query.Where(r => r.Rating == rating);
            }

            // Áp dụng bộ lọc "Có hình ảnh"
            if (withPhotos)
            {
                // Điều kiện này giả sử ImageUrl sẽ không phải là giá trị mặc định nếu có ảnh
                // Hoặc bạn có thể có một trường boolean IsImageUploaded
                query = query.Where(r => r.ImageUrl != null && r.ImageUrl != "/images/default-postImage.png" && r.ImageUrl != "");
            }

            // Áp dụng sắp xếp
            switch (sortBy.ToLower())
            {
                case "oldest":
                    query = query.OrderBy(r => r.CreatedAt);
                    break;
                case "highest":
                    query = query.OrderByDescending(r => r.Rating).ThenByDescending(r => r.CreatedAt);
                    break;
                case "lowest":
                    query = query.OrderBy(r => r.Rating).ThenBy(r => r.CreatedAt);
                    break;
                case "newest":
                default:
                    query = query.OrderByDescending(r => r.CreatedAt);
                    break;
            }
            return query;
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

        // // POST: TouristSpots/Edit/5
        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // public async Task<IActionResult> Edit(int id, [Bind("SpotId,Name,Address,CategoryId,Description,ImageUrl,CreatedAt")] TouristSpot touristSpot, IFormFile imageFile)
        // {
        //     if (id != touristSpot.SpotId)
        //     {
        //         return NotFound();
        //     }

        //     // Lấy thông tin hiện tại từ database để giữ lại các giá trị không thay đổi
        //     var existingSpot = await _context.TouristSpots.AsNoTracking().FirstOrDefaultAsync(s => s.SpotId == id);
        //     if (existingSpot == null)
        //     {
        //         return NotFound();
        //     }

        //     // Giữ lại URL ảnh cũ nếu không có ảnh mới được tải lên
        //     if (imageFile == null || imageFile.Length == 0)
        //     {
        //         touristSpot.ImageUrl = existingSpot.ImageUrl;
        //     }

        //     if (ModelState.IsValid)
        //     {
        //         try
        //         {
        //             // Xử lý tải lên hình ảnh mới nếu có
        //             if (imageFile != null && imageFile.Length > 0)
        //             {
        //                 // Kiểm tra kích thước file (ví dụ: tối đa 5MB)
        //                 if (imageFile.Length > 5 * 1024 * 1024)
        //                 {
        //                     ModelState.AddModelError("ImageFile", "Kích thước file không được vượt quá 5MB");
        //                     ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", touristSpot.CategoryId);
        //                     return View(touristSpot);
        //                 }

        //                 // Kiểm tra loại file
        //                 var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        //                 var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
        //                 if (!allowedExtensions.Contains(extension))
        //                 {
        //                     ModelState.AddModelError("ImageFile", "Chỉ chấp nhận file ảnh có định dạng: .jpg, .jpeg, .png, .gif");
        //                     ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", touristSpot.CategoryId);
        //                     return View(touristSpot);
        //                 }
                        
        //                 // Tạo tên file duy nhất để tránh trùng lặp
        //                 string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                        
        //                 // Đường dẫn lưu file (trong thư mục wwwroot/images)
        //                 string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                        
        //                 // Đảm bảo thư mục tồn tại
        //                 if (!Directory.Exists(uploadsFolder))
        //                 {
        //                     Directory.CreateDirectory(uploadsFolder);
        //                 }
                        
        //                 string filePath = Path.Combine(uploadsFolder, fileName);
                        
        //                 // Lưu file vào thư mục
        //                 using (var fileStream = new FileStream(filePath, FileMode.Create))
        //                 {
        //                     await imageFile.CopyToAsync(fileStream);
        //                 }
                        
        //                 // Xóa ảnh cũ nếu không phải ảnh mặc định
        //                 var oldImagePath = touristSpot.ImageUrl;
        //                 if (!string.IsNullOrEmpty(oldImagePath) && !oldImagePath.Contains("default-spotImage.png"))
        //                 {
        //                     var oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, oldImagePath.TrimStart('/'));
        //                     if (System.IO.File.Exists(oldFilePath))
        //                     {
        //                         System.IO.File.Delete(oldFilePath);
        //                     }
        //                 }
                        
        //                 // Cập nhật đường dẫn ảnh mới trong model
        //                 touristSpot.ImageUrl = "/images/" + fileName;
        //             }
                    
        //             _context.Update(touristSpot);
        //             await _context.SaveChangesAsync();
        //             return RedirectToAction(nameof(Index));
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
        //     }
        //     ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", touristSpot.CategoryId);
        //     return View(touristSpot);
        // }
        // POST: TouristSpots/Edit/5
// POST: TouristSpots/Edit/5
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Edit(int id, [Bind("SpotId,Name,Address,CategoryId,Description,ImageUrl")] TouristSpot touristSpotFromForm, IFormFile imageFile)
{
    if (id != touristSpotFromForm.SpotId)
    {
        return NotFound();
    }

    // Luôn chuẩn bị lại ViewData cho DropDownList
    ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", touristSpotFromForm.CategoryId);

    if (ModelState.IsValid)
    {
        try
        {
            var touristSpotToUpdate = await _context.TouristSpots.FindAsync(id);

            if (touristSpotToUpdate == null)
            {
                // Log hoặc debug ở đây để xem tại sao không tìm thấy
                // _logger.LogWarning($"TouristSpot with id {id} not found for update.");
                return NotFound();
            }

            // Gán các giá trị từ form vào thực thể đã tải từ DB
            touristSpotToUpdate.Name = touristSpotFromForm.Name;
            touristSpotToUpdate.Address = touristSpotFromForm.Address;
            touristSpotToUpdate.CategoryId = touristSpotFromForm.CategoryId;
            touristSpotToUpdate.Description = touristSpotFromForm.Description;
            // KHÔNG CẬP NHẬT CreatedAt
            // ImageUrl sẽ được xử lý bên dưới

            // Xử lý tải lên hình ảnh mới nếu có
            if (imageFile != null && imageFile.Length > 0)
            {
                // ... (toàn bộ code xử lý imageFile như đã cung cấp)
                // ... (bao gồm xóa ảnh cũ và cập nhật touristSpotToUpdate.ImageUrl)
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
                 var oldImagePathFromDb = touristSpotToUpdate.ImageUrl; // Lấy từ thực thể đang theo dõi
                 if (!string.IsNullOrEmpty(oldImagePathFromDb) && !oldImagePathFromDb.Contains("default-spotImage.png"))
                 {
                     var oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, oldImagePathFromDb.TrimStart('/'));
                     if (System.IO.File.Exists(oldFilePath))
                     {
                         System.IO.File.Delete(oldFilePath);
                     }
                 }
                 // Cập nhật đường dẫn ảnh mới trong model
                 touristSpotToUpdate.ImageUrl = "/images/" + fileName;
            }
            else
            {
                // Nếu không có file ảnh mới, đảm bảo ImageUrl được giữ nguyên từ giá trị đã có trong form
                // (hoặc giá trị đã được tải vào touristSpotToUpdate nếu bạn không muốn nó thay đổi từ hidden input)
                // Với hidden input asp-for="ImageUrl", touristSpotFromForm.ImageUrl sẽ chứa URL hiện tại.
                touristSpotToUpdate.ImageUrl = touristSpotFromForm.ImageUrl;
            }

            // EF Core tự động theo dõi thay đổi trên touristSpotToUpdate
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!TouristSpotExists(touristSpotFromForm.SpotId))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }
        // Bắt các exception khác nếu cần để debug
        catch (Exception ex)
        {
            // Log lỗi này
            // _logger.LogError(ex, "Error saving tourist spot.");
            ModelState.AddModelError("", "Có lỗi xảy ra khi lưu. Vui lòng thử lại.");
            // Trả về view với model từ form để người dùng không mất dữ liệu đã nhập
            return View(touristSpotFromForm);
        }
    }

    // Nếu ModelState không hợp lệ, trả về view với model từ form
    return View(touristSpotFromForm);
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
