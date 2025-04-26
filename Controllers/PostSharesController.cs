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
            ViewData["PostId"] = new SelectList(_context.Posts, "PostId", "Content");
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "Email");
            return View();
        }

        // POST: PostShares/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ShareId,UserId,PostId,SharedOn,SharedAt")] PostShare postShare)
        {
            if (ModelState.IsValid)
            {
                _context.Add(postShare);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PostId"] = new SelectList(_context.Posts, "PostId", "Content", postShare.PostId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "Email", postShare.UserId);
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
            ViewData["PostId"] = new SelectList(_context.Posts, "PostId", "Content", postShare.PostId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "Email", postShare.UserId);
            return View(postShare);
        }

        // POST: PostShares/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ShareId,UserId,PostId,SharedOn,SharedAt")] PostShare postShare)
        {
            if (id != postShare.ShareId)
            {
                return NotFound();
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
            ViewData["PostId"] = new SelectList(_context.Posts, "PostId", "Content", postShare.PostId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "Email", postShare.UserId);
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
    }
}
