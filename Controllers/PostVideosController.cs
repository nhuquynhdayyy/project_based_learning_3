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
    public class PostVideosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PostVideosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/PostVideos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PostVideo>>> GetPostVideos()
        {
            return await _context.PostVideos.ToListAsync();
        }

        // GET: api/PostVideos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PostVideo>> GetPostVideo(int id)
        {
            var postVideo = await _context.PostVideos.FindAsync(id);

            if (postVideo == null)
            {
                return NotFound();
            }

            return postVideo;
        }

        // PUT: api/PostVideos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPostVideo(int id, PostVideo postVideo)
        {
            if (id != postVideo.PostVideoId)
            {
                return BadRequest();
            }

            _context.Entry(postVideo).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PostVideoExists(id))
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

        // POST: api/PostVideos
        [HttpPost]
        public async Task<ActionResult<PostVideo>> PostPostVideo(PostVideo postVideo)
        {
            _context.PostVideos.Add(postVideo);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPostVideo", new { id = postVideo.PostVideoId }, postVideo);
        }

        // DELETE: api/PostVideos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePostVideo(int id)
        {
            var postVideo = await _context.PostVideos.FindAsync(id);
            if (postVideo == null)
            {
                return NotFound();
            }

            _context.PostVideos.Remove(postVideo);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PostVideoExists(int id)
        {
            return _context.PostVideos.Any(e => e.PostVideoId == id);
        }
    }
}
