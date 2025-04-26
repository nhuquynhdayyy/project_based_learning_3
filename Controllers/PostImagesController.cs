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
    public class PostImagesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PostImagesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: PostImages
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.PostImages.Include(p => p.Post);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: PostImages/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var postImage = await _context.PostImages
                .Include(p => p.Post)
                .FirstOrDefaultAsync(m => m.PostImageId == id);
            if (postImage == null)
            {
                return NotFound();
            }

            return View(postImage);
        }

        // GET: PostImages/Create
        public IActionResult Create()
        {
            ViewData["PostId"] = new SelectList(_context.Posts, "PostId", "Content");
            return View();
        }

        // POST: PostImages/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PostImageId,PostId,ImageUrl")] PostImage postImage)
        {
            if (ModelState.IsValid)
            {
                _context.Add(postImage);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PostId"] = new SelectList(_context.Posts, "PostId", "Content", postImage.PostId);
            return View(postImage);
        }

        // GET: PostImages/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var postImage = await _context.PostImages.FindAsync(id);
            if (postImage == null)
            {
                return NotFound();
            }
            ViewData["PostId"] = new SelectList(_context.Posts, "PostId", "Content", postImage.PostId);
            return View(postImage);
        }

        // POST: PostImages/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PostImageId,PostId,ImageUrl")] PostImage postImage)
        {
            if (id != postImage.PostImageId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(postImage);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostImageExists(postImage.PostImageId))
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
            ViewData["PostId"] = new SelectList(_context.Posts, "PostId", "Content", postImage.PostId);
            return View(postImage);
        }

        // GET: PostImages/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var postImage = await _context.PostImages
                .Include(p => p.Post)
                .FirstOrDefaultAsync(m => m.PostImageId == id);
            if (postImage == null)
            {
                return NotFound();
            }

            return View(postImage);
        }

        // POST: PostImages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var postImage = await _context.PostImages.FindAsync(id);
            if (postImage != null)
            {
                _context.PostImages.Remove(postImage);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PostImageExists(int id)
        {
            return _context.PostImages.Any(e => e.PostImageId == id);
        }
    }
}
