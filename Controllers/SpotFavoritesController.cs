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
    public class SpotFavoritesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SpotFavoritesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: SpotFavorites
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.SpotFavorites.Include(s => s.Spot).Include(s => s.User);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: SpotFavorites/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var spotFavorite = await _context.SpotFavorites
                .Include(s => s.Spot)
                .Include(s => s.User)
                .FirstOrDefaultAsync(m => m.FavoriteId == id);
            if (spotFavorite == null)
            {
                return NotFound();
            }

            return View(spotFavorite);
        }

        // GET: SpotFavorites/Create
        public IActionResult Create()
        {
            ViewData["SpotId"] = new SelectList(_context.TouristSpots, "SpotId", "Name");
            return View();
        }

        // POST: SpotFavorites/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FavoriteId,SpotId,CreatedAt")] SpotFavorite spotFavorite)
        {
            if (ModelState.IsValid)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized();
                }

                spotFavorite.UserId = int.Parse(userIdClaim.Value);
                spotFavorite.CreatedAt = DateTime.Now;

                _context.Add(spotFavorite);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["SpotId"] = new SelectList(_context.TouristSpots, "SpotId", "Name", spotFavorite.SpotId);
            return View(spotFavorite);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateViaAjax([FromBody] SpotFavoriteAjaxModel model)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            var userId = int.Parse(userIdClaim.Value);

            // Kiểm tra xem đã yêu thích chưa -> Nếu rồi thì xóa (toggle)
            var existing = await _context.SpotFavorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.SpotId == model.SpotId);

            if (existing != null)
            {
                _context.SpotFavorites.Remove(existing);
            }
            else
            {
                var spotFavorite = new SpotFavorite
                {
                    UserId = userId,
                    SpotId = model.SpotId,
                    CreatedAt = DateTime.Now
                };
                _context.SpotFavorites.Add(spotFavorite);
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        public class SpotFavoriteAjaxModel
        {
            public int SpotId { get; set; }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("/SpotFavorites/ToggleFavorite/{id}")]
        public async Task<IActionResult> ToggleFavorite([FromRoute] int id)
        {
            var touristSpot = await _context.TouristSpots
                .Include(t => t.Favorites)
                .FirstOrDefaultAsync(t => t.SpotId == id);

            if (touristSpot == null)
            {
                return NotFound();
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var currentUserId))
            {
                return Unauthorized();
            }

            var existingFavorite = touristSpot.Favorites?.FirstOrDefault(f => f.UserId == currentUserId);

            if (existingFavorite != null)
            {
                _context.SpotFavorites.Remove(existingFavorite);
            }
            else
            {
                _context.SpotFavorites.Add(new SpotFavorite
                {
                    SpotId = id,
                    UserId = currentUserId,
                    CreatedAt = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();

            var likeCount = await _context.SpotFavorites.CountAsync(f => f.SpotId == id);

            return Json(new
            {
                success = true,
                favorited = existingFavorite == null,
                likeCount = likeCount
            });
        }


        // GET: SpotFavorites/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var spotFavorite = await _context.SpotFavorites.FindAsync(id);
            if (spotFavorite == null)
            {
                return NotFound();
            }
            ViewData["SpotId"] = new SelectList(_context.TouristSpots, "SpotId", "Name", spotFavorite.SpotId);
            return View(spotFavorite);
        }

        // POST: SpotFavorites/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("FavoriteId,SpotId,CreatedAt")] SpotFavorite spotFavorite)
        {
            if (id != spotFavorite.FavoriteId)
            {
                return NotFound();
            }
            // Gán lại UserId từ Claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized(); // người dùng chưa đăng nhập
            spotFavorite.UserId = int.Parse(userIdClaim.Value); // Gán lại UserId từ Claims

            // Kiểm tra xem UserId có tồn tại trong bảng Users không
            var userExists = await _context.Users.AnyAsync(u => u.UserId == spotFavorite.UserId);
            if (!userExists)
            {
                return NotFound("User does not exist.");
            }
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(spotFavorite);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SpotFavoriteExists(spotFavorite.FavoriteId))
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
            ViewData["SpotId"] = new SelectList(_context.TouristSpots, "SpotId", "Name", spotFavorite.SpotId);
            return View(spotFavorite);
        }

        // GET: SpotFavorites/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var spotFavorite = await _context.SpotFavorites
                .Include(s => s.Spot)
                .Include(s => s.User)
                .FirstOrDefaultAsync(m => m.FavoriteId == id);
            if (spotFavorite == null)
            {
                return NotFound();
            }

            return View(spotFavorite);
        }

        // POST: SpotFavorites/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var spotFavorite = await _context.SpotFavorites.FindAsync(id);
            if (spotFavorite != null)
            {
                _context.SpotFavorites.Remove(spotFavorite);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SpotFavoriteExists(int id)
        {
            return _context.SpotFavorites.Any(e => e.FavoriteId == id);
        }
    }
}
