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
    public class SpotImagesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SpotImagesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: SpotImages
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.SpotImages.Include(s => s.Spot).Include(s => s.User);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: SpotImages/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var spotImage = await _context.SpotImages
                .Include(s => s.Spot)
                .Include(s => s.User)
                .FirstOrDefaultAsync(m => m.ImageId == id);
            if (spotImage == null)
            {
                return NotFound();
            }

            return View(spotImage);
        }

        // GET: SpotImages/Create
        public IActionResult Create()
        {
            ViewData["SpotId"] = new SelectList(_context.TouristSpots, "SpotId", "Name");
            return View();
        }

        // POST: SpotImages/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ImageId,SpotId,ImageUrl,UploadedAt")] SpotImage spotImage)
        {
            if (ModelState.IsValid)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized();
                }

                spotImage.UploadedBy = int.Parse(userIdClaim.Value);
                spotImage.UploadedAt = DateTime.Now;

                if (string.IsNullOrEmpty(spotImage.ImageUrl))
                {
                    spotImage.ImageUrl = "/images/default-postImage.png";
                }
                _context.Add(spotImage);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["SpotId"] = new SelectList(_context.TouristSpots, "SpotId", "Name", spotImage.SpotId);
            return View(spotImage);
        }

        // GET: SpotImages/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var spotImage = await _context.SpotImages.FindAsync(id);
            if (spotImage == null)
            {
                return NotFound();
            }
            ViewData["SpotId"] = new SelectList(_context.TouristSpots, "SpotId", "Name", spotImage.SpotId);
            return View(spotImage);
        }

        // POST: SpotImages/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ImageId,SpotId,ImageUrl,UploadedAt")] SpotImage spotImage)
        {
            if (id != spotImage.ImageId)
            {
                return NotFound();
            }
            // Gán lại UserId từ Claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized(); // người dùng chưa đăng nhập
            spotImage.UploadedBy = int.Parse(userIdClaim.Value); // Gán lại UserId từ Claims

            // Kiểm tra xem UserId có tồn tại trong bảng Users không
            var userExists = await _context.Users.AnyAsync(u => u.UserId == spotImage.UploadedBy);
            if (!userExists)
            {
                return NotFound("User does not exist.");
            }
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(spotImage);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SpotImageExists(spotImage.ImageId))
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
            ViewData["SpotId"] = new SelectList(_context.TouristSpots, "SpotId", "Name", spotImage.SpotId);
            return View(spotImage);
        }

        // GET: SpotImages/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var spotImage = await _context.SpotImages
                .Include(s => s.Spot)
                .Include(s => s.User)
                .FirstOrDefaultAsync(m => m.ImageId == id);
            if (spotImage == null)
            {
                return NotFound();
            }

            return View(spotImage);
        }

        // POST: SpotImages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var spotImage = await _context.SpotImages.FindAsync(id);
            if (spotImage != null)
            {
                _context.SpotImages.Remove(spotImage);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SpotImageExists(int id)
        {
            return _context.SpotImages.Any(e => e.ImageId == id);
        }
    }
}
