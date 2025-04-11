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
    public class PostImagesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PostImagesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/PostImages
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PostImage>>> GetPostImages()
        {
            return await _context.PostImages.ToListAsync();
        }

        // GET: api/PostImages/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PostImage>> GetPostImage(int id)
        {
            var postImage = await _context.PostImages.FindAsync(id);

            if (postImage == null)
            {
                return NotFound();
            }

            return postImage;
        }

        // PUT: api/PostImages/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPostImage(int id, PostImage postImage)
        {
            if (id != postImage.PostImageId)
            {
                return BadRequest();
            }

            _context.Entry(postImage).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PostImageExists(id))
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

        // POST: api/PostImages
        [HttpPost]
        public async Task<ActionResult<PostImage>> PostPostImage(PostImage postImage)
        {
            _context.PostImages.Add(postImage);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPostImage", new { id = postImage.PostImageId }, postImage);
        }

        // DELETE: api/PostImages/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePostImage(int id)
        {
            var postImage = await _context.PostImages.FindAsync(id);
            if (postImage == null)
            {
                return NotFound();
            }

            _context.PostImages.Remove(postImage);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PostImageExists(int id)
        {
            return _context.PostImages.Any(e => e.PostImageId == id);
        }
    }
}
