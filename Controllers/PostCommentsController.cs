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
    public class PostCommentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PostCommentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/PostComments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PostComment>>> GetPostComments()
        {
            return await _context.PostComments
                .Include(c => c.User)
                .Include(c => c.Post)
                .ToListAsync();
        }

        // GET: api/PostComments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PostComment>> GetPostComment(int id)
        {
            var comment = await _context.PostComments
                .Include(c => c.User)
                .Include(c => c.Post)
                .FirstOrDefaultAsync(c => c.CommentId == id);

            if (comment == null)
            {
                return NotFound();
            }

            return comment;
        }

        // GET: api/PostComments/by-post/3
        [HttpGet("by-post/{postId}")]
        public async Task<ActionResult<IEnumerable<PostComment>>> GetCommentsByPost(int postId)
        {
            return await _context.PostComments
                .Where(c => c.PostId == postId)
                .Include(c => c.User)
                .ToListAsync();
        }

        // PUT: api/PostComments/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPostComment(int id, PostComment comment)
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

        // POST: api/PostComments
        [HttpPost]
        public async Task<ActionResult<PostComment>> PostPostComment(PostComment comment)
        {
            comment.CreatedAt = DateTime.UtcNow;

            _context.PostComments.Add(comment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPostComment), new { id = comment.CommentId }, comment);
        }

        // DELETE: api/PostComments/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePostComment(int id)
        {
            var comment = await _context.PostComments.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }

            _context.PostComments.Remove(comment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CommentExists(int id)
        {
            return _context.PostComments.Any(e => e.CommentId == id);
        }
    }
}
