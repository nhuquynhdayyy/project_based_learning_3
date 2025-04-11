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
    public class SpotCommentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SpotCommentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/SpotComments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SpotComment>>> GetComments()
        {
            return await _context.SpotComments
                .Include(c => c.User)
                .Include(c => c.Spot)
                .ToListAsync();
        }

        // GET: api/SpotComments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SpotComment>> GetComment(int id)
        {
            var comment = await _context.SpotComments
                .Include(c => c.User)
                .Include(c => c.Spot)
                .FirstOrDefaultAsync(c => c.CommentId == id);

            if (comment == null)
            {
                return NotFound();
            }

            return comment;
        }

        // PUT: api/SpotComments/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutComment(int id, SpotComment comment)
        {
            if (id != comment.CommentId)
            {
                return BadRequest();
            }

            _context.Entry(comment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CommentExists(id))
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

        // POST: api/SpotComments
        [HttpPost]
        public async Task<ActionResult<SpotComment>> PostComment(SpotComment comment)
        {
            comment.CreatedAt = DateTime.UtcNow;

            _context.SpotComments.Add(comment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetComment), new { id = comment.CommentId }, comment);
        }

        // DELETE: api/SpotComments/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var comment = await _context.SpotComments.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }

            _context.SpotComments.Remove(comment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CommentExists(int id)
        {
            return _context.SpotComments.Any(e => e.CommentId == id);
        }
    }
}
