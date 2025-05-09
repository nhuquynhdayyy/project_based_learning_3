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
            ViewData["SpotId"] = new SelectList(_context.TouristSpots, "SpotId", "Name");
            return View();
        }

        // POST: SpotShares/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ShareId,SpotId,SharedOn,SharedAt")] SpotShare spotShare)
        {
            if (ModelState.IsValid)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized();
                }

                spotShare.UserId = int.Parse(userIdClaim.Value);
                spotShare.SharedAt = DateTime.Now;
                _context.Add(spotShare);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["SpotId"] = new SelectList(_context.TouristSpots, "SpotId", "Name", spotShare.SpotId);
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
            ViewData["SpotId"] = new SelectList(_context.TouristSpots, "SpotId", "Name", spotShare.SpotId);
            return View(spotShare);
        }

        // POST: SpotShares/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ShareId,SpotId,SharedOn,SharedAt")] SpotShare spotShare)
        {
            if (id != spotShare.ShareId)
            {
                return NotFound();
            }
            // Gán lại UserId từ Claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized(); // người dùng chưa đăng nhập
            spotShare.UserId = int.Parse(userIdClaim.Value); // Gán lại UserId từ Claims

            // Kiểm tra xem UserId có tồn tại trong bảng Users không
            var userExists = await _context.Users.AnyAsync(u => u.UserId == spotShare.UserId);
            if (!userExists)
            {
                return NotFound("User does not exist.");
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
            ViewData["SpotId"] = new SelectList(_context.TouristSpots, "SpotId", "Name", spotShare.SpotId);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Share([FromBody] ShareRequestDto request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var userId = int.Parse(userIdClaim.Value);

            // Validate platform
            var allowedPlatforms = new[] { "Facebook", "Twitter", "Instagram", "Zalo" };
            if (!allowedPlatforms.Contains(request.SharedOn))
            {
                return BadRequest("Nền tảng không hợp lệ.");
            }

            var share = new SpotShare
            {
                SpotId = request.SpotId,
                UserId = userId,
                SharedOn = request.SharedOn,
                SharedAt = DateTime.Now
            };

            _context.SpotShares.Add(share);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        public class ShareRequestDto
        {
            public int SpotId { get; set; }
            public string SharedOn { get; set; }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFromShare([FromBody] ShareRequestModel model)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized();

            if (string.IsNullOrEmpty(model.Platform) || model.SpotId == 0)
                return BadRequest("Missing data.");

            var share = new SpotShare
            {
                UserId = int.Parse(userIdClaim.Value),
                SpotId = model.SpotId,
                SharedOn = model.Platform,
                SharedAt = DateTime.Now
            };

            _context.SpotShares.Add(share);
            await _context.SaveChangesAsync();

            // Gửi lại đường link trang chi tiết để JS chia sẻ tiếp
            var spotUrl = Url.Action("Details", "TouristSpots", new { id = model.SpotId }, Request.Scheme);
            return Json(new { success = true, url = spotUrl });
        }

        public class ShareRequestModel
        {
            public string Platform { get; set; }
            public int SpotId { get; set; }
        }

    }
}
