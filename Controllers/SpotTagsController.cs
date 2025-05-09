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
    public class SpotTagsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SpotTagsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: SpotTags
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.SpotTags.Include(s => s.Spot).Include(s => s.Tag);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: SpotTags/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int? spotId, int? tagId)
        {
            if (spotId == null || tagId == null)
            {
                return NotFound();
            }

            var spotTag = await _context.SpotTags
                .Include(s => s.Spot)
                .Include(s => s.Tag)
                .FirstOrDefaultAsync(m => m.SpotId == spotId && m.TagId == tagId);

            if (spotTag == null)
            {
                return NotFound();
            }

            return View(spotTag);
        }


        // GET: SpotTags/Create
        public IActionResult Create()
        {
            ViewData["SpotId"] = new SelectList(_context.TouristSpots, "SpotId", "Name");
            ViewData["TagId"] = new SelectList(_context.Tags, "TagId", "Name");
            return View();
        }

        // POST: SpotTags/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SpotId,TagId")] SpotTag spotTag)
        {
            if (ModelState.IsValid)
            {
                _context.Add(spotTag);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            else {
                ModelState.AddModelError("", "Invalid data. Please check your input.");
                
            }
            ViewData["SpotId"] = new SelectList(_context.TouristSpots, "SpotId", "Name", spotTag.SpotId);
            ViewData["TagId"] = new SelectList(_context.Tags, "TagId", "Name", spotTag.TagId);
            return View(spotTag);
        }

        // GET: SpotTags/Edit/5
        // GET: SpotTags/Edit/spotId/tagId
        [HttpGet("SpotTags/Edit/{spotId}/{tagId}")]
        public async Task<IActionResult> Edit(int? spotId, int? tagId)
        {
            if (spotId == null || tagId == null)
            {
                return NotFound();
            }

            var spotTag = await _context.SpotTags
                .Include(s => s.Spot)
                .Include(s => s.Tag)
                .FirstOrDefaultAsync(m => m.SpotId == spotId && m.TagId == tagId);

            if (spotTag == null)
            {
                return NotFound();
            }

            ViewData["SpotId"] = new SelectList(_context.TouristSpots, "SpotId", "Name", spotTag.SpotId);
            ViewData["TagId"] = new SelectList(_context.Tags, "TagId", "Name", spotTag.TagId);

            return View(spotTag);
        }

        // POST: SpotTags/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("SpotTags/Edit/{spotId}/{tagId}")]
        public async Task<IActionResult> Edit(int spotId, int tagId, [Bind("SpotId,TagId")] SpotTag spotTag)
        {
            if (spotId != spotTag.SpotId || tagId != spotTag.TagId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(spotTag);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SpotTagExists(spotTag.SpotId, spotTag.TagId))
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

            ViewData["SpotId"] = new SelectList(_context.TouristSpots, "SpotId", "Name", spotTag.SpotId);
            ViewData["TagId"] = new SelectList(_context.Tags, "TagId", "Name", spotTag.TagId);

            return View(spotTag);
        }

        private bool SpotTagExists(int spotId, int tagId)
        {
            return _context.SpotTags.Any(e => e.SpotId == spotId && e.TagId == tagId);
        }


        // GET: SpotTags/Delete
        // GET: SpotTags/Delete
        [HttpGet]
        public async Task<IActionResult> Delete(int? spotId, int? tagId)
        {
            if (spotId == null || tagId == null)
            {
                return NotFound();
            }

            var spotTag = await _context.SpotTags
                .Include(s => s.Spot)
                .Include(s => s.Tag)
                .FirstOrDefaultAsync(m => m.SpotId == spotId && m.TagId == tagId);

            if (spotTag == null)
            {
                return NotFound();
            }

            return View(spotTag);
        }

        // POST: SpotTags/DeleteConfirmed
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int spotId, int tagId)
        {
            var spotTag = await _context.SpotTags.FindAsync(spotId, tagId);
            if (spotTag != null)
            {
                _context.SpotTags.Remove(spotTag);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
