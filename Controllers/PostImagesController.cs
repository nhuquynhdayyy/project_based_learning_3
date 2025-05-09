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
    public class PostImagesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PostImagesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: PostImages
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.PostImages.Include(p => p.Post).Include(p => p.User);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: PostImages/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var postImage = await _context.PostImages
                .Include(p => p.Post)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.PostImageId == id);
            if (postImage == null)
            {
                return NotFound();
            }

            return View(postImage);
        }

        // GET: PostImages/Create
        public IActionResult Create()
        {
            ViewData["PostId"] = new SelectList(_context.Posts, "PostId", "Title");
            return View();
        }

        // POST: PostImages/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PostImageId,PostId,ImageUrl,UploadedBy,UploadedAt")] PostImage postImage)
        {
            if (ModelState.IsValid)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized();
                }

                postImage.UploadedBy = int.Parse(userIdClaim.Value);
                postImage.UploadedAt = DateTime.Now;

                if (string.IsNullOrEmpty(postImage.ImageUrl))
                {
                    postImage.ImageUrl = "/images/default-postImage.png";
                }
                _context.Add(postImage);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PostId"] = new SelectList(_context.Posts, "PostId", "Title", postImage.PostId);
            return View(postImage);
        }

        // GET: PostImages/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var postImage = await _context.PostImages.FindAsync(id);
            if (postImage == null)
            {
                return NotFound();
            }
            ViewData["PostId"] = new SelectList(_context.Posts, "PostId", "Title", postImage.PostId);
            return View(postImage);
        }

        // POST: PostImages/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PostImageId,PostId,ImageUrl,UploadedAt")] PostImage postImage)
        {
            if (id != postImage.PostImageId)
            {
                return NotFound();
            }
            // Gán lại UserId từ Claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized(); // người dùng chưa đăng nhập
            postImage.UploadedBy = int.Parse(userIdClaim.Value); // Gán lại UserId từ Claims

            // Kiểm tra xem UserId có tồn tại trong bảng Users không
            var userExists = await _context.Users.AnyAsync(u => u.UserId == postImage.UploadedBy);
            if (!userExists)
            {
                return NotFound("User does not exist.");
            }
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(postImage);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostImageExists(postImage.PostImageId))
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
            ViewData["PostId"] = new SelectList(_context.Posts, "PostId", "Title", postImage.PostId);
            return View(postImage);
        }

        // GET: PostImages/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var postImage = await _context.PostImages
                .Include(p => p.Post)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.PostImageId == id);
            if (postImage == null)
            {
                return NotFound();
            }

            return View(postImage);
        }

        // POST: PostImages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var postImage = await _context.PostImages.FindAsync(id);
            if (postImage != null)
            {
                _context.PostImages.Remove(postImage);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PostImageExists(int id)
        {
            return _context.PostImages.Any(e => e.PostImageId == id);
        }
    }
}
