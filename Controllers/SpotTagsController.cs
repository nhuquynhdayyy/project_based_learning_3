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
    public class SpotTagsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SpotTagsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/SpotTags
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SpotTag>>> GetSpotTags()
        {
            return await _context.SpotTags
                .Include(st => st.Spot)
                .Include(st => st.Tag)
                .ToListAsync();
        }

        // POST: api/SpotTags
        [HttpPost]
        public async Task<ActionResult<SpotTag>> PostSpotTag(SpotTag spotTag)
        {
            _context.SpotTags.Add(spotTag);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSpotTags), new { spotId = spotTag.SpotId, tagId = spotTag.TagId }, spotTag);
        }

        // DELETE: api/SpotTags/spotId/tagId
        [HttpDelete("{spotId}/{tagId}")]
        public async Task<IActionResult> DeleteSpotTag(int spotId, int tagId)
        {
            var spotTag = await _context.SpotTags.FindAsync(spotId, tagId);
            if (spotTag == null)
            {
                return NotFound();
            }

            _context.SpotTags.Remove(spotTag);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
