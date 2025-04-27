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
    public class PostTagsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PostTagsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: PostTags
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.PostTags.Include(p => p.Post).Include(p => p.Tag);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: PostTags/Details/5
        [HttpGet]
        [Route("PostTags/Details/{postId}/{tagId}")]
        public async Task<IActionResult> Details(int? postId, int? tagId)
        {
            if (postId == null || tagId == null)
            {
                return NotFound();
            }

            var postTag = await _context.PostTags
                .Include(p => p.Post)
                .Include(p => p.Tag)
                .FirstOrDefaultAsync(m => m.PostId == postId && m.TagId == tagId);

            if (postTag == null)
            {
                return NotFound();
            }

            return View(postTag);
        }

        // GET: PostTags/Create
        public IActionResult Create()
        {
            ViewData["PostId"] = new SelectList(_context.Posts, "PostId", "Title");
            ViewData["TagId"] = new SelectList(_context.Tags, "TagId", "Name");
            return View();
        }

        // POST: PostTags/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PostId,TagId")] PostTag postTag)
        {
            if (ModelState.IsValid)
            {
                _context.Add(postTag);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PostId"] = new SelectList(_context.Posts, "PostId", "Title", postTag.PostId);
            ViewData["TagId"] = new SelectList(_context.Tags, "TagId", "Name", postTag.TagId);
            return View(postTag);
        }

        // GET: PostTags/Edit/5
        [HttpGet]
        [Route("PostTags/Edit/{postId}/{tagId}")]
        public async Task<IActionResult> Edit(int? postId, int? tagId)
        {
            if (postId == null || tagId == null)
            {
                return NotFound();
            }

            var postTag = await _context.PostTags.FindAsync(postId, tagId);
            if (postTag == null)
            {
                return NotFound();
            }
            ViewData["PostId"] = new SelectList(_context.Posts, "PostId", "Title", postTag.PostId);
            ViewData["TagId"] = new SelectList(_context.Tags, "TagId", "Name", postTag.TagId);
            return View(postTag);
        }

        // POST: PostTags/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Route("PostTags/Edit/{postId}/{tagId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int postId, int tagId, [Bind("PostId,TagId")] PostTag postTag)
        {
            if (postId != postTag.PostId || tagId != postTag.TagId)
            {
                return NotFound();
            }

            // Check if the postTag exists in the database
            var existingPostTag = await _context.PostTags.FindAsync(postId, tagId);
            if (existingPostTag == null)
            {
                return NotFound();
            }

            // Update the properties of the existing postTag
            existingPostTag.TagId = postTag.TagId;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(existingPostTag);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostTagExists(postTag.PostId))
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
            ViewData["PostId"] = new SelectList(_context.Posts, "PostId", "Title", postTag.PostId);
            ViewData["TagId"] = new SelectList(_context.Tags, "TagId", "Name", postTag.TagId);
            return View(postTag);
        }
        

        // GET: PostTags/Delete/{postId}/{tagId}
        [HttpGet]
        [Route("PostTags/Delete/{postId}/{tagId}")]
        public async Task<IActionResult> Delete(int? postId, int? tagId)
        {
            if (postId == null || tagId == null)
            {
                return NotFound();
            }

            var postTag = await _context.PostTags
                .Include(p => p.Post)
                .Include(p => p.Tag)
                .FirstOrDefaultAsync(m => m.PostId == postId && m.TagId == tagId);
            if (postTag == null)
            {
                return NotFound();
            }

            return View(postTag);
        }

        // POST: PostTags/DeleteConfirmed/{postId}/{tagId}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("PostTags/DeleteConfirmed/{postId}/{tagId}")]
        public async Task<IActionResult> DeleteConfirmed(int postId, int tagId)
        {
            var postTag = await _context.PostTags.FindAsync(postId, tagId);
            if (postTag != null)
            {
                _context.PostTags.Remove(postTag);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }


        private bool PostTagExists(int id)
        {
            return _context.PostTags.Any(e => e.PostId == id);
        }
    }
}
