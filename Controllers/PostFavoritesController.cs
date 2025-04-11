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
    public class PostFavoritesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PostFavoritesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/PostFavorites
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PostFavorite>>> GetPostFavorites()
        {
            return await _context.PostFavorites
                .Include(f => f.User)
                .Include(f => f.Post)
                .ToListAsync();
        }

        // GET: api/PostFavorites/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PostFavorite>> GetPostFavorite(int id)
        {
            var favorite = await _context.PostFavorites
                .Include(f => f.User)
                .Include(f => f.Post)
                .FirstOrDefaultAsync(f => f.FavoriteId == id);

            if (favorite == null)
            {
                return NotFound();
            }

            return favorite;
        }

        // GET: api/PostFavorites/by-user/3
        [HttpGet("by-user/{userId}")]
        public async Task<ActionResult<IEnumerable<PostFavorite>>> GetFavoritesByUser(int userId)
        {
            return await _context.PostFavorites
                .Where(f => f.UserId == userId)
                .Include(f => f.Post)
                .ToListAsync();
        }

        // POST: api/PostFavorites
        [HttpPost]
        public async Task<ActionResult<PostFavorite>> PostPostFavorite(PostFavorite favorite)
        {
            favorite.CreatedAt = DateTime.UtcNow;

            _context.PostFavorites.Add(favorite);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPostFavorite), new { id = favorite.FavoriteId }, favorite);
        }

        // DELETE: api/PostFavorites/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePostFavorite(int id)
        {
            var favorite = await _context.PostFavorites.FindAsync(id);
            if (favorite == null)
            {
                return NotFound();
            }

            _context.PostFavorites.Remove(favorite);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PostFavoriteExists(int id)
        {
            return _context.PostFavorites.Any(e => e.FavoriteId == id);
        }
    }
}
