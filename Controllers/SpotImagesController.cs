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
    public class SpotImagesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SpotImagesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/SpotImages
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SpotImage>>> GetSpotImages()
        {
            return await _context.SpotImages
                .Include(i => i.Spot)
                .Include(i => i.UploadedBy)
                .ToListAsync();
        }

        // GET: api/SpotImages/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SpotImage>> GetSpotImage(int id)
        {
            var spotImage = await _context.SpotImages
                .Include(i => i.Spot)
                .Include(i => i.UploadedBy)
                .FirstOrDefaultAsync(i => i.ImageId == id);

            if (spotImage == null)
            {
                return NotFound();
            }

            return spotImage;
        }

        // PUT: api/SpotImages/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSpotImage(int id, SpotImage spotImage)
        {
            if (id != spotImage.ImageId)
            {
                return BadRequest();
            }

            _context.Entry(spotImage).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SpotImageExists(id))
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

        // POST: api/SpotImages
        [HttpPost]
        public async Task<ActionResult<SpotImage>> PostSpotImage(SpotImage spotImage)
        {
            spotImage.UploadedAt = DateTime.UtcNow;

            _context.SpotImages.Add(spotImage);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSpotImage), new { id = spotImage.ImageId }, spotImage);
        }

        // DELETE: api/SpotImages/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSpotImage(int id)
        {
            var spotImage = await _context.SpotImages.FindAsync(id);
            if (spotImage == null)
            {
                return NotFound();
            }

            _context.SpotImages.Remove(spotImage);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SpotImageExists(int id)
        {
            return _context.SpotImages.Any(e => e.ImageId == id);
        }
    }
}
