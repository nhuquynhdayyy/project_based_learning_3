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
    public class SpotSharesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SpotSharesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/SpotShares
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SpotShare>>> GetSpotShares()
        {
            return await _context.SpotShares
                .Include(s => s.User)
                .Include(s => s.Spot)
                .ToListAsync();
        }

        // GET: api/SpotShares/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SpotShare>> GetSpotShare(int id)
        {
            var spotShare = await _context.SpotShares
                .Include(s => s.User)
                .Include(s => s.Spot)
                .FirstOrDefaultAsync(s => s.ShareId == id);

            if (spotShare == null)
            {
                return NotFound();
            }

            return spotShare;
        }

        // POST: api/SpotShares
        [HttpPost]
        public async Task<ActionResult<SpotShare>> PostSpotShare(SpotShare spotShare)
        {
            spotShare.SharedAt = DateTime.UtcNow;

            _context.SpotShares.Add(spotShare);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSpotShare), new { id = spotShare.ShareId }, spotShare);
        }

        // PUT: api/SpotShares/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSpotShare(int id, SpotShare spotShare)
        {
            if (id != spotShare.ShareId)
            {
                return BadRequest();
            }

            _context.Entry(spotShare).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SpotShareExists(id))
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

        // DELETE: api/SpotShares/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSpotShare(int id)
        {
            var spotShare = await _context.SpotShares.FindAsync(id);
            if (spotShare == null)
            {
                return NotFound();
            }

            _context.SpotShares.Remove(spotShare);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SpotShareExists(int id)
        {
            return _context.SpotShares.Any(e => e.ShareId == id);
        }
    }
}
