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
    public class PostCommentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PostCommentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: PostComments
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.PostComments.Include(p => p.Post).Include(p => p.User);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: PostComments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var postComment = await _context.PostComments
                .Include(p => p.Post)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.CommentId == id);
            if (postComment == null)
            {
                return NotFound();
            }

            return View(postComment);
        }

        // GET: PostComments/Create
        public IActionResult Create()
        {
            ViewData["PostId"] = new SelectList(_context.Posts, "PostId", "Title");
            return View();
        }

        // POST: PostComments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CommentId,PostId,Content,ImageUrl,CreatedAt")] PostComment postComment)
        {
            if (ModelState.IsValid)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized();
                }

                postComment.UserId = int.Parse(userIdClaim.Value);
                postComment.CreatedAt = DateTime.Now;

                if (string.IsNullOrEmpty(postComment.ImageUrl))
                {
                    postComment.ImageUrl = "/images/default-postImage.png";
                }
                _context.Add(postComment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PostId"] = new SelectList(_context.Posts, "PostId", "Content", postComment.PostId);
            return View(postComment);
        }

        // GET: PostComments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var postComment = await _context.PostComments.FindAsync(id);
            if (postComment == null)
            {
                return NotFound();
            }
            ViewData["PostId"] = new SelectList(_context.Posts, "PostId", "Content", postComment.PostId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "FullName", postComment.UserId);
            return View(postComment);
        }

        // POST: PostComments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CommentId,UserId,PostId,Content,ImageUrl,CreatedAt")] PostComment postComment)
        {
            if (id != postComment.CommentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(postComment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostCommentExists(postComment.CommentId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["PostId"] = new SelectList(_context.Posts, "PostId", "Content", postComment.PostId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "FullName", postComment.UserId);
            return View(postComment);
        }

        // GET: PostComments/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var postComment = await _context.PostComments
                .Include(p => p.Post)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.CommentId == id);
            if (postComment == null)
            {
                return NotFound();
            }

            // Kiểm tra quyền xóa comment
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            var currentUserId = userIdClaim?.Value;

            // Kiểm tra xem người dùng có phải là chủ sở hữu comment HOẶC là Admin không
            bool isOwner = (currentUserId != null && postComment.UserId.ToString() == currentUserId);
            bool isAdmin = User.IsInRole("Admin");

            if (!isOwner && !isAdmin)
            {
                // Nếu không phải chủ comment và cũng không phải Admin thì từ chối
                return Unauthorized("Bạn không có quyền thực hiện hành động này.");
            }
            // if (userIdClaim == null || postComment.UserId != int.Parse(userIdClaim.Value))
            // {
            //     return Unauthorized();
            // }

            return View(postComment);
        }

        // POST: PostComments/Delete/5
        // [HttpPost, ActionName("Delete")]
        // [ValidateAntiForgeryToken]
        // public async Task<IActionResult> DeleteConfirmed(int id)
        // {
        //     var postComment = await _context.PostComments.FindAsync(id);
        //     int postId = 0;

        //     if (postComment != null)
        //     {
        //         // _context.PostComments.Remove(postComment);

        //         // Lưu lại PostId trước khi xóa comment
        //         postId = postComment.PostId;
                
        //         // Kiểm tra quyền xóa comment
        //         var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        //         if (userIdClaim != null && postComment.UserId == int.Parse(userIdClaim.Value))
        //         {
        //             _context.PostComments.Remove(postComment);
        //             await _context.SaveChangesAsync();
        //         }
        //     }

        //     // await _context.SaveChangesAsync();
            
        //     // Trở về trang chi tiết bài viết sau khi xóa comment
        //     if (postId > 0)
        //     {
        //         return RedirectToAction("Details", "Posts", new { id = postId });
        //     }

        //     return RedirectToAction(nameof(Index));
        // }
        // POST: PostComments/Delete/5
[HttpPost, ActionName("Delete")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> DeleteConfirmed(int id)
{
    var postComment = await _context.PostComments.FindAsync(id);

    // Nếu không tìm thấy comment, trả về NotFound
    if (postComment == null)
    {
        return NotFound();
    }

    // Luôn lưu lại PostId để có thể chuyển hướng về đúng bài viết
    int postId = postComment.PostId;

    // --- Bắt đầu phần kiểm tra quyền ---
    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
    var currentUserId = userIdClaim?.Value;

    // Điều kiện 1: Người dùng có phải là chủ sở hữu comment không?
    // Chú ý: Chuyển đổi UserId (int) sang string để so sánh an toàn với claim (string)
    bool isOwner = (currentUserId != null && postComment.UserId.ToString() == currentUserId);

    // Điều kiện 2: Người dùng có phải là Admin không?
    bool isAdmin = User.IsInRole("Admin");

    // Nếu người dùng là chủ sở hữu HOẶC là Admin thì cho phép xóa
    if (isOwner || isAdmin)
    {
        _context.PostComments.Remove(postComment);
        await _context.SaveChangesAsync();
    }
    else
    {
        // Nếu không có quyền, trả về lỗi Unauthorized để người dùng biết
        return Unauthorized("Bạn không có quyền thực hiện hành động này.");
    }
    // --- Kết thúc phần kiểm tra quyền ---
    
    // Chuyển hướng người dùng về trang chi tiết bài viết sau khi xóa thành công
    return RedirectToAction("Details", "Posts", new { id = postId });
}

        private bool PostCommentExists(int id)
        {
            return _context.PostComments.Any(e => e.CommentId == id);
        }
    }
}
