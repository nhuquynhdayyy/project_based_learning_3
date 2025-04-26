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
            ViewData["PostId"] = new SelectList(_context.Posts, "PostId", "Content");
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "Email");
            return View();
        }

        // POST: PostFavorites/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FavoriteId,UserId,PostId,CreatedAt")] PostFavorite postFavorite)
        {
            if (ModelState.IsValid)
            {
                _context.Add(postFavorite);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PostId"] = new SelectList(_context.Posts, "PostId", "Content", postFavorite.PostId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "Email", postFavorite.UserId);
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
            ViewData["PostId"] = new SelectList(_context.Posts, "PostId", "Content", postFavorite.PostId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "Email", postFavorite.UserId);
            return View(postFavorite);
        }

        // POST: PostFavorites/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("FavoriteId,UserId,PostId,CreatedAt")] PostFavorite postFavorite)
        {
            if (id != postFavorite.FavoriteId)
            {
                return NotFound();
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
            ViewData["PostId"] = new SelectList(_context.Posts, "PostId", "Content", postFavorite.PostId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "Email", postFavorite.UserId);
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
    }
}
