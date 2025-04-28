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
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PostsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Posts
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Posts.Include(p => p.Spot).Include(p => p.User);
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
                .FirstOrDefaultAsync(m => m.PostId == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // GET: Posts/Create
        public IActionResult Create()
        {
            ViewData["SpotId"] = new SelectList(_context.TouristSpots, "SpotId", "Name");
            return View();
        }

        // POST: Posts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SpotId,TypeOfPost,Title,ImageUrl,Content")] Post post)
        {
            if (ModelState.IsValid)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized();
                }

                post.UserId = int.Parse(userIdClaim.Value);
                post.CreatedAt = DateTime.Now;

                if (string.IsNullOrEmpty(post.ImageUrl))
                {
                    post.ImageUrl = "/images/default-postImage.png";
                }

                _context.Add(post);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["SpotId"] = new SelectList(_context.TouristSpots, "SpotId", "Name", post.SpotId);
            return View(post);
        }


        // GET: Posts/Edit/5
        
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }
            ViewData["SpotId"] = new SelectList(_context.TouristSpots, "SpotId", "Name", post.SpotId);
            return View(post);
        }


        // POST: Posts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PostId,SpotId,TypeOfPost,Title,Content,ImageUrl")] Post post)
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

                    var existingPost = await _context.Posts.FirstOrDefaultAsync(p => p.PostId == id);
                    if (existingPost == null)
                    {
                        return NotFound();
                    }

                    // Kiểm tra người dùng có quyền sửa bài viết này không
                    if (existingPost.UserId != int.Parse(userIdClaim.Value))
                    {
                        return Unauthorized();
                    }

                    // Cập nhật từng field cho existingPost
                    existingPost.SpotId = post.SpotId;
                    existingPost.TypeOfPost = post.TypeOfPost;
                    existingPost.Title = post.Title;
                    existingPost.Content = post.Content;
                    existingPost.ImageUrl = string.IsNullOrEmpty(post.ImageUrl) 
                        ? existingPost.ImageUrl ?? "/images/default-postImage.png" 
                        : post.ImageUrl;
                        
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
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
        [HttpGet]
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

            return View(post);
        }

        // POST: Posts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post != null)
            {
                _context.Posts.Remove(post);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PostExists(int id)
        {
            return _context.Posts.Any(e => e.PostId == id);
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
                .Where(p => p.TypeOfPost == type)
                .ToListAsync();

            ViewBag.TypeOfPost = type; // Gửi loại bài viết để hiện lên tiêu đề nếu cần
            return View("Index", posts); // Xài lại view Index.cshtml
        }
    }
}
