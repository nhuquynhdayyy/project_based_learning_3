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
    public class SpotFavoritesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SpotFavoritesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/SpotFavorites
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SpotFavorite>>> GetSpotFavorites()
        {
            return await _context.SpotFavorites
                .Include(f => f.User)
                .Include(f => f.Spot)
                .ToListAsync();
        }

        // GET: api/SpotFavorites/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SpotFavorite>> GetSpotFavorite(int id)
        {
            var favorite = await _context.SpotFavorites
                .Include(f => f.User)
                .Include(f => f.Spot)
                .FirstOrDefaultAsync(f => f.FavoriteId == id);

            if (favorite == null)
            {
                return NotFound();
            }

            return favorite;
        }

        // GET: api/SpotFavorites/by-user/3
        [HttpGet("by-user/{userId}")]
        public async Task<ActionResult<IEnumerable<SpotFavorite>>> GetFavoritesByUser(int userId)
        {
            return await _context.SpotFavorites
                .Where(f => f.UserId == userId)
                .Include(f => f.Spot)
                .ToListAsync();
        }

        // POST: api/SpotFavorites
        [HttpPost]
        public async Task<ActionResult<SpotFavorite>> PostSpotFavorite(SpotFavorite favorite)
        {
            favorite.CreatedAt = DateTime.UtcNow;

            _context.SpotFavorites.Add(favorite);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSpotFavorite), new { id = favorite.FavoriteId }, favorite);
        }

        // DELETE: api/SpotFavorites/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSpotFavorite(int id)
        {
            var favorite = await _context.SpotFavorites.FindAsync(id);
            if (favorite == null)
            {
                return NotFound();
            }

            _context.SpotFavorites.Remove(favorite);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SpotFavoriteExists(int id)
        {
            return _context.SpotFavorites.Any(e => e.FavoriteId == id);
        }
    }
}
