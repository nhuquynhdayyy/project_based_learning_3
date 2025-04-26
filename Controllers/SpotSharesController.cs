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
    public class SpotSharesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SpotSharesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: SpotShares
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.SpotShares.Include(s => s.Spot).Include(s => s.User);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: SpotShares/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var spotShare = await _context.SpotShares
                .Include(s => s.Spot)
                .Include(s => s.User)
                .FirstOrDefaultAsync(m => m.ShareId == id);
            if (spotShare == null)
            {
                return NotFound();
            }

            return View(spotShare);
        }

        // GET: SpotShares/Create
        public IActionResult Create()
        {
            ViewData["SpotId"] = new SelectList(_context.TouristSpots, "SpotId", "Address");
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "Email");
            return View();
        }

        // POST: SpotShares/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ShareId,UserId,SpotId,SharedOn,SharedAt")] SpotShare spotShare)
        {
            if (ModelState.IsValid)
            {
                _context.Add(spotShare);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["SpotId"] = new SelectList(_context.TouristSpots, "SpotId", "Address", spotShare.SpotId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "Email", spotShare.UserId);
            return View(spotShare);
        }

        // GET: SpotShares/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var spotShare = await _context.SpotShares.FindAsync(id);
            if (spotShare == null)
            {
                return NotFound();
            }
            ViewData["SpotId"] = new SelectList(_context.TouristSpots, "SpotId", "Address", spotShare.SpotId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "Email", spotShare.UserId);
            return View(spotShare);
        }

        // POST: SpotShares/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ShareId,UserId,SpotId,SharedOn,SharedAt")] SpotShare spotShare)
        {
            if (id != spotShare.ShareId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(spotShare);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SpotShareExists(spotShare.ShareId))
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
            ViewData["SpotId"] = new SelectList(_context.TouristSpots, "SpotId", "Address", spotShare.SpotId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "Email", spotShare.UserId);
            return View(spotShare);
        }

        // GET: SpotShares/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var spotShare = await _context.SpotShares
                .Include(s => s.Spot)
                .Include(s => s.User)
                .FirstOrDefaultAsync(m => m.ShareId == id);
            if (spotShare == null)
            {
                return NotFound();
            }

            return View(spotShare);
        }

        // POST: SpotShares/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var spotShare = await _context.SpotShares.FindAsync(id);
            if (spotShare != null)
            {
                _context.SpotShares.Remove(spotShare);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SpotShareExists(int id)
        {
            return _context.SpotShares.Any(e => e.ShareId == id);
        }
    }
}
