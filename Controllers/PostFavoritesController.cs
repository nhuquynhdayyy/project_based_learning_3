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
    public class PostFavoritesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PostFavoritesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: PostFavorites
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.PostFavorites.Include(p => p.Post).Include(p => p.User);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: PostFavorites/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var postFavorite = await _context.PostFavorites
                .Include(p => p.Post)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.FavoriteId == id);
            if (postFavorite == null)
            {
                return NotFound();
            }

            return View(postFavorite);
        }

        // GET: PostFavorites/Create
        public IActionResult Create()
        {
            ViewData["PostId"] = new SelectList(_context.Posts, "PostId", "Title");
            return View();
        }

        // POST: PostFavorites/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FavoriteId,PostId,CreatedAt")] PostFavorite postFavorite)
        {
            if (ModelState.IsValid)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized();
                }

                postFavorite.UserId = int.Parse(userIdClaim.Value);
                postFavorite.CreatedAt = DateTime.Now;

                _context.Add(postFavorite);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PostId"] = new SelectList(_context.Posts, "PostId", "Title", postFavorite.PostId);
            return View(postFavorite);
        }

        // GET: PostFavorites/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var postFavorite = await _context.PostFavorites.FindAsync(id);
            if (postFavorite == null)
            {
                return NotFound();
            }
            ViewData["PostId"] = new SelectList(_context.Posts, "PostId", "Title", postFavorite.PostId);
            return View(postFavorite);
        }

        // POST: PostFavorites/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("FavoriteId,PostId,CreatedAt")] PostFavorite postFavorite)
        {
            if (id != postFavorite.FavoriteId)
            {
                return NotFound();
            }
            // Gán lại UserId từ Claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized(); // người dùng chưa đăng nhập
            postFavorite.UserId = int.Parse(userIdClaim.Value); // Gán lại UserId từ Claims

            // Kiểm tra xem UserId có tồn tại trong bảng Users không
            var userExists = await _context.Users.AnyAsync(u => u.UserId == postFavorite.UserId);
            if (!userExists)
            {
                return NotFound("User does not exist.");
            }
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(postFavorite);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostFavoriteExists(postFavorite.FavoriteId))
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
            ViewData["PostId"] = new SelectList(_context.Posts, "PostId", "Title", postFavorite.PostId);
            return View(postFavorite);
        }

        // GET: PostFavorites/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var postFavorite = await _context.PostFavorites
                .Include(p => p.Post)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.FavoriteId == id);
            if (postFavorite == null)
            {
                return NotFound();
            }

            return View(postFavorite);
        }

        // POST: PostFavorites/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var postFavorite = await _context.PostFavorites.FindAsync(id);
            if (postFavorite != null)
            {
                _context.PostFavorites.Remove(postFavorite);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PostFavoriteExists(int id)
        {
            return _context.PostFavorites.Any(e => e.FavoriteId == id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("/PostFavorites/ToggleFavorite/{id}")]
        public async Task<IActionResult> ToggleFavorite([FromRoute] int id)
        {
            var post = await _context.Posts
                .Include(t => t.PostFavorites)
                .FirstOrDefaultAsync(t => t.PostId == id);

            if (post == null)
            {
                return NotFound();
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var currentUserId))
            {
                return Unauthorized();
            }

            var existingFavorite = post.PostFavorites?.FirstOrDefault(f => f.UserId == currentUserId);

            if (existingFavorite != null)
            {
                _context.PostFavorites.Remove(existingFavorite);
            }
            else
            {
                _context.PostFavorites.Add(new PostFavorite
                {
                    PostId = id,
                    UserId = currentUserId,
                    CreatedAt = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();

            var likeCount = await _context.PostFavorites.CountAsync(f => f.PostId == id);

            return Json(new
            {
                success = true,
                favorited = existingFavorite == null,
                likeCount = likeCount
            });
        }

    }
}
