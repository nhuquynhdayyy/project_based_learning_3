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
    public class PostSharesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PostSharesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: PostShares
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.PostShares.Include(p => p.Post).Include(p => p.User);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: PostShares/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var postShare = await _context.PostShares
                .Include(p => p.Post)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.ShareId == id);
            if (postShare == null)
            {
                return NotFound();
            }

            return View(postShare);
        }

        // GET: PostShares/Create
        public IActionResult Create()
        {
            ViewData["PostId"] = new SelectList(_context.Posts, "PostId", "Title");
            return View();
        }

        // POST: PostShares/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ShareId,PostId,SharedOn,SharedAt")] PostShare postShare)
        {
            if (ModelState.IsValid)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized();
                }

                postShare.UserId = int.Parse(userIdClaim.Value);
                postShare.SharedAt = DateTime.Now;

                _context.Add(postShare);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PostId"] = new SelectList(_context.Posts, "PostId", "Title", postShare.PostId);
            return View(postShare);
        }

        // GET: PostShares/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var postShare = await _context.PostShares.FindAsync(id);
            if (postShare == null)
            {
                return NotFound();
            }
            ViewData["PostId"] = new SelectList(_context.Posts, "PostId", "Title", postShare.PostId);
            return View(postShare);
        }

        // POST: PostShares/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ShareId,PostId,SharedOn,SharedAt")] PostShare postShare)
        {
            if (id != postShare.ShareId)
            {
                return NotFound();
            }
            // Gán lại UserId từ Claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized(); // người dùng chưa đăng nhập
            postShare.UserId = int.Parse(userIdClaim.Value); // Gán lại UserId từ Claims

            // Kiểm tra xem UserId có tồn tại trong bảng Users không
            var userExists = await _context.Users.AnyAsync(u => u.UserId == postShare.UserId);
            if (!userExists)
            {
                return NotFound("User does not exist.");
            }
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(postShare);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostShareExists(postShare.ShareId))
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
            ViewData["PostId"] = new SelectList(_context.Posts, "PostId", "Title", postShare.PostId);
            return View(postShare);
        }

        // GET: PostShares/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var postShare = await _context.PostShares
                .Include(p => p.Post)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.ShareId == id);
            if (postShare == null)
            {
                return NotFound();
            }

            return View(postShare);
        }

        // POST: PostShares/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var postShare = await _context.PostShares.FindAsync(id);
            if (postShare != null)
            {
                _context.PostShares.Remove(postShare);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PostShareExists(int id)
        {
            return _context.PostShares.Any(e => e.ShareId == id);
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

            var share = new PostShare
            {
                PostId = request.PostId,
                UserId = userId,
                SharedOn = request.SharedOn,
                SharedAt = DateTime.Now
            };

            _context.PostShares.Add(share);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        public class ShareRequestDto
        {
            public int PostId { get; set; }
            public string SharedOn { get; set; }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFromShare([FromBody] ShareRequestModel model)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized();

            if (string.IsNullOrEmpty(model.Platform) || model.PostId == 0)
                return BadRequest("Missing data.");

            var share = new PostShare
            {
                UserId = int.Parse(userIdClaim.Value),
                PostId = model.PostId,
                SharedOn = model.Platform,
                SharedAt = DateTime.Now
            };

            _context.PostShares.Add(share);
            await _context.SaveChangesAsync();

            // Gửi lại đường link trang chi tiết để JS chia sẻ tiếp
            var postUrl = Url.Action("Details", "Posts", new { id = model.PostId }, Request.Scheme);
            return Json(new { success = true, url = postUrl });
        }

        public class ShareRequestModel
        {
            public string Platform { get; set; }
            public int PostId { get; set; }
        }
    }
}
