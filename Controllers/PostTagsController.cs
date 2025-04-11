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
    public class PostTagsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PostTagsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/PostTags
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PostTag>>> GetPostTags()
        {
            return await _context.PostTags
                .Include(pt => pt.Post)
                .Include(pt => pt.Tag)
                .ToListAsync();
        }

        // POST: api/PostTags
        [HttpPost]
        public async Task<ActionResult<PostTag>> PostPostTag(PostTag postTag)
        {
            _context.PostTags.Add(postTag);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPostTags), new { postId = postTag.PostId, tagId = postTag.TagId }, postTag);
        }

        // DELETE: api/PostTags/postId/tagId
        [HttpDelete("{postId}/{tagId}")]
        public async Task<IActionResult> DeletePostTag(int postId, int tagId)
        {
            var postTag = await _context.PostTags.FindAsync(postId, tagId);
            if (postTag == null)
            {
                return NotFound();
            }

            _context.PostTags.Remove(postTag);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
