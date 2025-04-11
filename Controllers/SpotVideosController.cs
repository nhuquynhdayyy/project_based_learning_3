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
    public class SpotVideosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SpotVideosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/SpotVideos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SpotVideo>>> GetSpotVideos()
        {
            return await _context.SpotVideos
                .Include(v => v.Spot)
                .Include(v => v.UploadedBy)
                .ToListAsync();
        }

        // GET: api/SpotVideos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SpotVideo>> GetSpotVideo(int id)
        {
            var spotVideo = await _context.SpotVideos
                .Include(v => v.Spot)
                .Include(v => v.UploadedBy)
                .FirstOrDefaultAsync(v => v.VideoId == id);

            if (spotVideo == null)
            {
                return NotFound();
            }

            return spotVideo;
        }

        // PUT: api/SpotVideos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSpotVideo(int id, SpotVideo spotVideo)
        {
            if (id != spotVideo.VideoId)
            {
                return BadRequest();
            }

            _context.Entry(spotVideo).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SpotVideoExists(id))
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

        // POST: api/SpotVideos
        [HttpPost]
        public async Task<ActionResult<SpotVideo>> PostSpotVideo(SpotVideo spotVideo)
        {
            spotVideo.UploadedAt = DateTime.UtcNow;

            _context.SpotVideos.Add(spotVideo);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSpotVideo), new { id = spotVideo.VideoId }, spotVideo);
        }

        // DELETE: api/SpotVideos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSpotVideo(int id)
        {
            var spotVideo = await _context.SpotVideos.FindAsync(id);
            if (spotVideo == null)
            {
                return NotFound();
            }

            _context.SpotVideos.Remove(spotVideo);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SpotVideoExists(int id)
        {
            return _context.SpotVideos.Any(e => e.VideoId == id);
        }
    }
}
