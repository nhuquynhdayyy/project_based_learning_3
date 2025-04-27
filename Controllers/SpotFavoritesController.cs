using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TourismWeb.Models;

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
            ViewData["SpotId"] = new SelectList(_context.TouristSpots, "SpotId", "Address");
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "FullName");
            return View();
        }

        // POST: SpotFavorites/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FavoriteId,UserId,SpotId,CreatedAt")] SpotFavorite spotFavorite)
        {
            if (ModelState.IsValid)
            {
                _context.Add(spotFavorite);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["SpotId"] = new SelectList(_context.TouristSpots, "SpotId", "Address", spotFavorite.SpotId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "FullName", spotFavorite.UserId);
            return View(spotFavorite);
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
            ViewData["SpotId"] = new SelectList(_context.TouristSpots, "SpotId", "Address", spotFavorite.SpotId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "FullName", spotFavorite.UserId);
            return View(spotFavorite);
        }

        // POST: SpotFavorites/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("FavoriteId,UserId,SpotId,CreatedAt")] SpotFavorite spotFavorite)
        {
            if (id != spotFavorite.FavoriteId)
            {
                return NotFound();
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
            ViewData["SpotId"] = new SelectList(_context.TouristSpots, "SpotId", "Address", spotFavorite.SpotId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "FullName", spotFavorite.UserId);
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
