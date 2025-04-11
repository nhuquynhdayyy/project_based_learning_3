using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourismWeb.Models;

namespace TourismWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostSharesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PostSharesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/PostShares
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PostShare>>> GetPostShares()
        {
            return await _context.PostShares
                .Include(s => s.User)
                .Include(s => s.Post)
                .ToListAsync();
        }

        // GET: api/PostShares/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PostShare>> GetPostShare(int id)
        {
            var postShare = await _context.PostShares
                .Include(s => s.User)
                .Include(s => s.Post)
                .FirstOrDefaultAsync(s => s.ShareId == id);

            if (postShare == null)
            {
                return NotFound();
            }

            return postShare;
        }

        // POST: api/PostShares
        [HttpPost]
        public async Task<ActionResult<PostShare>> PostPostShare(PostShare postShare)
        {
            postShare.SharedAt = DateTime.UtcNow;

            _context.PostShares.Add(postShare);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPostShare), new { id = postShare.ShareId }, postShare);
        }

        // PUT: api/PostShares/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPostShare(int id, PostShare postShare)
        {
            if (id != postShare.ShareId)
            {
                return BadRequest();
            }

            _context.Entry(postShare).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PostShareExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/PostShares/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePostShare(int id)
        {
            var postShare = await _context.PostShares.FindAsync(id);
            if (postShare == null)
            {
                return NotFound();
            }

            _context.PostShares.Remove(postShare);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PostShareExists(int id)
        {
            return _context.PostShares.Any(e => e.ShareId == id);
        }
    }
}
