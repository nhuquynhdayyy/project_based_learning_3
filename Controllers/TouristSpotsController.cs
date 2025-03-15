using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourismWeb.Models;

namespace TourismWeb
{
    [Route("api/[controller]")]
    [ApiController]
    public class TouristSpotsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TouristSpotsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/TouristSpots
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TouristSpot>>> GetTouristSpots()
        {
            return await _context.TouristSpots.ToListAsync();
        }

        // GET: api/TouristSpots/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TouristSpot>> GetTouristSpot(int id)
        {
            var touristSpot = await _context.TouristSpots.FindAsync(id);

            if (touristSpot == null)
            {
                return NotFound();
            }

            return touristSpot;
        }

        // PUT: api/TouristSpots/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTouristSpot(int id, TouristSpot touristSpot)
        {
            if (id != touristSpot.SpotId)
            {
                return BadRequest();
            }

            _context.Entry(touristSpot).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TouristSpotExists(id))
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

        // POST: api/TouristSpots
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<TouristSpot>> PostTouristSpot(TouristSpot touristSpot)
        {
            _context.TouristSpots.Add(touristSpot);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTouristSpot", new { id = touristSpot.SpotId }, touristSpot);
        }

        // DELETE: api/TouristSpots/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTouristSpot(int id)
        {
            var touristSpot = await _context.TouristSpots.FindAsync(id);
            if (touristSpot == null)
            {
                return NotFound();
            }

            _context.TouristSpots.Remove(touristSpot);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TouristSpotExists(int id)
        {
            return _context.TouristSpots.Any(e => e.SpotId == id);
        }
    }
}
