using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TourismWeb.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using TourismWeb.Utilities; // Namespace chứa FileHelper (nếu bạn tạo)

namespace TourismWeb.Controllers
{
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment; // Để xử lý upload file

        // public PostsController(ApplicationDbContext context)
        // {
        //     _context = context;
        // }
        public PostsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }

        // GET: Posts - Show only approved posts for regular view
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Posts
                .Include(p => p.Spot)
                .Include(p => p.User)
                .Where(p => p.Status == PostStatus.Approved);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Posts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Spot)
                .Include(p => p.User)
                .Include(p => p.Images)
                .Include(p => p.PostFavorites)
                .Include(p => p.Comments) // Include comments if needed
                    .ThenInclude(c => c.User) // Include user for each comment
                .FirstOrDefaultAsync(m => m.PostId == id);

            if (post == null)
            {
                return NotFound();
            }

            // Sắp xếp bình luận theo thời gian tạo (mới nhất lên đầu)
            post.Comments = post.Comments.OrderByDescending(c => c.CreatedAt).ToList();

            // Check if the user is the author or an admin if the post is not approved
            if (post.Status != PostStatus.Approved)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                bool isAdmin = User.IsInRole("Admin");

                if (userIdClaim == null ||
                    (!isAdmin && post.UserId != int.Parse(userIdClaim.Value)))
                {
                    return NotFound(); // Hide pending/rejected posts from other users
                }
            }

            return View(post);
        }

        // GET: Posts/Create
        [Authorize]
        public IActionResult Create()
        {
            ViewData["SpotId"] = new SelectList(_context.TouristSpots, "SpotId", "Name");
            return View(new Post()); // Trả về một đối tượng Post mới
        }
    // --- Controller (PostsController.cs) ---
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(
    [Bind("PostId,SpotId,TypeOfPost,Title,Content,ImageUrl,EstimatedVisitTime,TicketPriceInfo,LocationRating,SuggestedItinerary,GuidebookSummary,TravelTips,PackingListSuggestions,EstimatedCosts,UsefulDocumentsHtml,ExperienceEndDate,Companions,ApproximateCost,OverallExperienceRating,RatingLandscape,RatingFood,RatingService,RatingPrice,ExperienceHighlights,ExperienceItinerarySummary,Advice")] Post post,
    IFormFile? imageFile,
    List<IFormFile>? additionalImages)
{
    var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
    {
        ModelState.AddModelError("", "Không thể xác định người dùng. Vui lòng đăng nhập lại.");
        ViewData["SpotId"] = new SelectList(_context.TouristSpots, "SpotId", "Name", post.SpotId);
        return View(post);
    }

    post.UserId = userId;
    post.CreatedAt = DateTime.Now;
    post.Status = PostStatus.Pending;

    // Xử lý ảnh bìa
    if (imageFile != null && imageFile.Length > 0)
    {
        try
        {
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "posts");
            post.ImageUrl = await FileHelper.SaveFileAsync(imageFile, uploadsFolder);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("ImageUrl", $"Lỗi khi tải ảnh lên: {ex.Message}");
        }
    }
    else if (string.IsNullOrEmpty(post.ImageUrl))
    {
        post.ImageUrl = "/images/default-postImage.png";
    }

    // Xử lý nhiều ảnh phụ
    if (additionalImages != null && additionalImages.Count > 0)
    {
        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "posts");

        foreach (var image in additionalImages)
{
    if (image != null && image.Length > 0)
    {
        try
        {
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(image.FileName);
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            post.Images.Add(new PostImage
            {
                ImageUrl = "/uploads/posts/" + uniqueFileName,
                UploadedBy = userId,
                UploadedAt = DateTime.Now
            });
        }
        catch (Exception ex)
        {
            // ✅ Log lỗi rõ ràng
            ModelState.AddModelError("", $"Lỗi khi lưu ảnh: {image.FileName} - {ex.Message}");
        }
    }
    else
    {
        ModelState.AddModelError("", $"Ảnh {image?.FileName ?? "không rõ"} bị rỗng hoặc lỗi khi chọn.");
    }
}

    }

    if (ModelState.IsValid)
    {
        _context.Add(post);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Tạo bài viết thành công! Bài viết đang chờ duyệt.";
        return RedirectToAction(nameof(Index));
    }

    ViewData["SpotId"] = new SelectList(_context.TouristSpots, "SpotId", "Name", post.SpotId);
    return View(post);
}


       
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.PostId == id);

            if (post == null)
            {
                return NotFound();
            }

            // Check if user is authorized to edit this post
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            bool isAdmin = User.IsInRole("Admin");

            if (userIdClaim == null ||
                (!isAdmin && post.UserId != int.Parse(userIdClaim.Value)))
            {
                return Unauthorized();
            }

            ViewData["SpotId"] = new SelectList(_context.TouristSpots, "SpotId", "Name", post.SpotId);
            return View(post);
        }

        // POST: Posts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [Bind("PostId,SpotId,TypeOfPost,Title,Content,ImageUrl,Status")] Post post, IFormFile mainImageFile, List<IFormFile> additionalImages, List<int> imagesToDelete)
        {
            if (id != post.PostId)
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

                    int userId = int.Parse(userIdClaim.Value);
                    bool isAdmin = User.IsInRole("Admin");

                    var existingPost = await _context.Posts
                        .Include(p => p.Images)
                        .FirstOrDefaultAsync(p => p.PostId == id);

                    if (existingPost == null)
                    {
                        return NotFound();
                    }

                    // Check if user has permission to edit this post
                    if (!isAdmin && existingPost.UserId != userId)
                    {
                        return Unauthorized();
                    }

                    // Update basic fields
                    existingPost.SpotId = post.SpotId;
                    existingPost.TypeOfPost = post.TypeOfPost;
                    existingPost.Title = post.Title;
                    existingPost.Content = post.Content;

                    // Process main image upload if provided
                    if (mainImageFile != null && mainImageFile.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(mainImageFile.FileName);
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await mainImageFile.CopyToAsync(fileStream);
                        }

                        existingPost.ImageUrl = "/images/" + uniqueFileName;
                    }
                    // Don't change to default if no new image is uploaded
                    else if (string.IsNullOrEmpty(post.ImageUrl) && string.IsNullOrEmpty(existingPost.ImageUrl))
                    {
                        existingPost.ImageUrl = "/images/default-postImage.png";
                    }

                    // Process additional images
                    if (additionalImages != null && additionalImages.Count > 0)
                    {
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        foreach (var imageFile in additionalImages)
                        {
                            if (imageFile != null && imageFile.Length > 0)
                            {
                                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
                                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                                using (var fileStream = new FileStream(filePath, FileMode.Create))
                                {
                                    await imageFile.CopyToAsync(fileStream);
                                }

                                PostImage newImage = new PostImage
                                {
                                    PostId = existingPost.PostId,
                                    ImageUrl = "/images/" + uniqueFileName,
                                    UploadedBy = userId,
                                    UploadedAt = DateTime.Now
                                };

                                _context.PostImages.Add(newImage);
                            }
                        }
                    }

                    // Delete images if specified
                    if (imagesToDelete != null && imagesToDelete.Count > 0)
                    {
                        foreach (var imageId in imagesToDelete)
                        {
                            var imageToRemove = await _context.PostImages.FindAsync(imageId);
                            if (imageToRemove != null &&
                                (isAdmin || imageToRemove.UploadedBy == userId) &&
                                imageToRemove.PostId == existingPost.PostId)
                            {
                                _context.PostImages.Remove(imageToRemove);
                            }
                        }
                    }

                    // Only admin can directly change status from the edit form
                    if (isAdmin && post.Status != existingPost.Status)
                    {
                        existingPost.Status = post.Status;
                    }
                    // For regular users editing their posts, reset to pending if already approved
                    else if (!isAdmin && existingPost.Status == PostStatus.Approved)
                    {
                        existingPost.Status = PostStatus.Pending;
                    }

                    await _context.SaveChangesAsync();

                    if (isAdmin)
                    {
                        return RedirectToAction(nameof(Moderate));
                    }
                    else
                    {
                        // Redirect to appropriate category page based on post type
                        if (!string.IsNullOrEmpty(existingPost.TypeOfPost))
                        {
                            return RedirectToAction(nameof(Category), new { type = existingPost.TypeOfPost });
                        }
                        return RedirectToAction(nameof(MyPosts));
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(post.PostId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewData["SpotId"] = new SelectList(_context.TouristSpots, "SpotId", "Name", post.SpotId);
            return View(post);
        }
        // GET: Posts/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Spot)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.PostId == id);

            if (post == null)
            {
                return NotFound();
            }

            // Check if user is authorized to delete this post
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            bool isAdmin = User.IsInRole("Admin");

            if (userIdClaim == null ||
                (!isAdmin && post.UserId != int.Parse(userIdClaim.Value)))
            {
                return Unauthorized();
            }

            return View(post);
        }

        // POST: Posts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            string postType = null;
            if (post == null)
            {
                return NotFound();
            }

            // Check if user is authorized to delete this post
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            bool isAdmin = User.IsInRole("Admin");

            if (userIdClaim == null ||
                (!isAdmin && post.UserId != int.Parse(userIdClaim.Value)))
            {
                return Unauthorized();
            }

            postType = post.TypeOfPost; // Store the type of post for redirection   
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            if (isAdmin)
            {
                return RedirectToAction(nameof(Moderate));
            }
            else
            {
                // Redirect to appropriate category page if we know the post type
                if (!string.IsNullOrEmpty(postType))
                {
                    return RedirectToAction(nameof(Category), new { type = postType });
                }
                return RedirectToAction(nameof(MyPosts));
            }
        }

        // GET: Posts/Category
        public async Task<IActionResult> Category(string type)
        {
            if (string.IsNullOrEmpty(type))
            {
                return RedirectToAction(nameof(Index));
            }

            var posts = await _context.Posts
                .Include(p => p.Spot)
                .Include(p => p.User)
                .Where(p => p.TypeOfPost == type && p.Status == PostStatus.Approved)
                .ToListAsync();

            ViewBag.TypeOfPost = type;
            // return View("Index", posts);
            // Use different views based on category type
            switch (type)
            {
                case "Cẩm nang":
                    return View("Guidebook", posts);
                case "Trải nghiệm":
                    return View("Experience", posts);
                case "Địa điểm":
                    return View("Location", posts);
                default:
                    return View("Index", posts);
            }
        }

        // GET: Posts/MyPosts - For users to see their own posts with any status
        [Authorize]
        public async Task<IActionResult> MyPosts()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            int userId = int.Parse(userIdClaim.Value);

            var posts = await _context.Posts
                .Include(p => p.Spot)
                .Include(p => p.User)
                .Where(p => p.UserId == userId)
                .ToListAsync();

            return View(posts);
        }


        // GET: Posts/Moderate - Admin only page to manage all posts
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Moderate(string status)
        {            
            IQueryable<Post> postsQuery = _context.Posts
            .Include(p => p.Spot)  // Nạp kèm thông tin Địa điểm
            .Include(p => p.User); // Nạp kèm thông tin Người dùng
            PostStatus parsedStatus = default; // Sẽ giữ giá trị enum nếu status hợp lệ
            bool isValidStatusFilter = false;  // Cờ để biết có bộ lọc status hợp lệ không
            ViewData["CurrentFilter"] = status;

            if (!string.IsNullOrEmpty(status))
        {
            if (Enum.TryParse<PostStatus>(status, true, out parsedStatus))
            {
                postsQuery = postsQuery.Where(p => p.Status == parsedStatus);
                 isValidStatusFilter = true;
            }
           
        }
        List<Post> posts;

         if (isValidStatusFilter) 
        {
            posts = await postsQuery
                .OrderByDescending(p => p.CreatedAt) // Đối với các view đã lọc, sắp xếp theo ngày tạo giảm dần
                .ToListAsync();
        }
        else // "Tất cả" hoặc nếu status không hợp lệ/rỗng
        {
            posts = await postsQuery
                .OrderBy(p => p.Status) // Thứ tự sắp xếp ban đầu cho "Tất cả"
                .ThenByDescending(p => p.CreatedAt)
                .ToListAsync();
        }
            return View(posts);
        }


        // POST: Posts/Approve/5 - Admin action to approve a post
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            post.Status = PostStatus.Approved;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Moderate));
        }

        // POST: Posts/Reject/5 - Admin action to reject a post
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reject(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            post.Status = PostStatus.Rejected;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Moderate));
        }

        private bool PostExists(int id)
        {
            return _context.Posts.Any(e => e.PostId == id);
        }

        private bool PostCommentExists(int id)
        {
            return _context.PostComments.Any(e => e.CommentId == id);
        }

        // POST: Posts/AddComment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int postId, string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return RedirectToAction(nameof(Details), new { id = postId });
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var userId = int.Parse(userIdClaim.Value);

            var postComment = new PostComment
            {
                PostId = postId,
                UserId = userId,
                Content = content,
                CreatedAt = DateTime.Now
            };

            _context.PostComments.Add(postComment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = postId });
        }
    }
}