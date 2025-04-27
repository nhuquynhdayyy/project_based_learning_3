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
    public class SpotCommentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SpotCommentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: SpotComments
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.SpotComments.Include(s => s.Spot).Include(s => s.User);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: SpotComments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var spotComment = await _context.SpotComments
                .Include(s => s.Spot)
                .Include(s => s.User)
                .FirstOrDefaultAsync(m => m.CommentId == id);
            if (spotComment == null)
            {
                return NotFound();
            }

            return View(spotComment);
        }

        // GET: SpotComments/Create
        public IActionResult Create()
        {
            ViewData["SpotId"] = new SelectList(_context.TouristSpots, "SpotId", "Name");
            return View();
        }

        // POST: SpotComments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CommentId,SpotId,Content,ImageUrl,CreatedAt")] SpotComment spotComment)
        {
            if (ModelState.IsValid)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized();
                }

                spotComment.UserId = int.Parse(userIdClaim.Value);
                spotComment.CreatedAt = DateTime.Now;

                if (string.IsNullOrEmpty(spotComment.ImageUrl))
                {
                    spotComment.ImageUrl = "/images/default-postImage.png";
                }
                _context.Add(spotComment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["SpotId"] = new SelectList(_context.TouristSpots, "SpotId", "Name", spotComment.SpotId);
            return View(spotComment);
        }

        // GET: SpotComments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var spotComment = await _context.SpotComments.FindAsync(id);
            if (spotComment == null)
            {
                return NotFound();
            }
            ViewData["SpotId"] = new SelectList(_context.TouristSpots, "SpotId", "Name", spotComment.SpotId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "FullName", spotComment.UserId);
            return View(spotComment);
        }

        // POST: SpotComments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CommentId,UserId,SpotId,Content,ImageUrl,CreatedAt")] SpotComment spotComment)
        {
            if (id != spotComment.CommentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(spotComment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SpotCommentExists(spotComment.CommentId))
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
            ViewData["SpotId"] = new SelectList(_context.TouristSpots, "SpotId", "Name", spotComment.SpotId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "FullName", spotComment.UserId);
            return View(spotComment);
        }

        // GET: SpotComments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var spotComment = await _context.SpotComments
                .Include(s => s.Spot)
                .Include(s => s.User)
                .FirstOrDefaultAsync(m => m.CommentId == id);
            if (spotComment == null)
            {
                return NotFound();
            }

            return View(spotComment);
        }

        // POST: SpotComments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var spotComment = await _context.SpotComments.FindAsync(id);
            if (spotComment != null)
            {
                _context.SpotComments.Remove(spotComment);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SpotCommentExists(int id)
        {
            return _context.SpotComments.Any(e => e.CommentId == id);
        }
    }
}
