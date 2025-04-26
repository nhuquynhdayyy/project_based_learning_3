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
    public class SpotVideosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SpotVideosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: SpotVideos
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.SpotVideos.Include(s => s.Spot).Include(s => s.User);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: SpotVideos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var spotVideo = await _context.SpotVideos
                .Include(s => s.Spot)
                .Include(s => s.User)
                .FirstOrDefaultAsync(m => m.VideoId == id);
            if (spotVideo == null)
            {
                return NotFound();
            }

            return View(spotVideo);
        }

        // GET: SpotVideos/Create
        public IActionResult Create()
        {
            ViewData["SpotId"] = new SelectList(_context.TouristSpots, "SpotId", "Address");
            ViewData["UploadedBy"] = new SelectList(_context.Users, "UserId", "Email");
            return View();
        }

        // POST: SpotVideos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("VideoId,SpotId,VideoUrl,UploadedBy,UploadedAt")] SpotVideo spotVideo)
        {
            if (ModelState.IsValid)
            {
                _context.Add(spotVideo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["SpotId"] = new SelectList(_context.TouristSpots, "SpotId", "Address", spotVideo.SpotId);
            ViewData["UploadedBy"] = new SelectList(_context.Users, "UserId", "Email", spotVideo.UploadedBy);
            return View(spotVideo);
        }

        // GET: SpotVideos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var spotVideo = await _context.SpotVideos.FindAsync(id);
            if (spotVideo == null)
            {
                return NotFound();
            }
            ViewData["SpotId"] = new SelectList(_context.TouristSpots, "SpotId", "Address", spotVideo.SpotId);
            ViewData["UploadedBy"] = new SelectList(_context.Users, "UserId", "Email", spotVideo.UploadedBy);
            return View(spotVideo);
        }

        // POST: SpotVideos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("VideoId,SpotId,VideoUrl,UploadedBy,UploadedAt")] SpotVideo spotVideo)
        {
            if (id != spotVideo.VideoId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(spotVideo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SpotVideoExists(spotVideo.VideoId))
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
            ViewData["SpotId"] = new SelectList(_context.TouristSpots, "SpotId", "Address", spotVideo.SpotId);
            ViewData["UploadedBy"] = new SelectList(_context.Users, "UserId", "Email", spotVideo.UploadedBy);
            return View(spotVideo);
        }

        // GET: SpotVideos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var spotVideo = await _context.SpotVideos
                .Include(s => s.Spot)
                .Include(s => s.User)
                .FirstOrDefaultAsync(m => m.VideoId == id);
            if (spotVideo == null)
            {
                return NotFound();
            }

            return View(spotVideo);
        }

        // POST: SpotVideos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var spotVideo = await _context.SpotVideos.FindAsync(id);
            if (spotVideo != null)
            {
                _context.SpotVideos.Remove(spotVideo);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SpotVideoExists(int id)
        {
            return _context.SpotVideos.Any(e => e.VideoId == id);
        }
    }
}
