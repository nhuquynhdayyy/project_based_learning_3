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
    public class SharesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SharesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Shares
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Share>>> GetShares()
        {
            return await _context.Shares.ToListAsync();
        }

        // GET: api/Shares/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Share>> GetShare(int id)
        {
            var share = await _context.Shares.FindAsync(id);

            if (share == null)
            {
                return NotFound();
            }

            return share;
        }

        // PUT: api/Shares/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutShare(int id, Share share)
        {
            if (id != share.ShareId)
            {
                return BadRequest();
            }

            _context.Entry(share).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ShareExists(id))
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

        // POST: api/Shares
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Share>> PostShare(Share share)
        {
            _context.Shares.Add(share);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetShare", new { id = share.ShareId }, share);
        }

        // DELETE: api/Shares/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteShare(int id)
        {
            var share = await _context.Shares.FindAsync(id);
            if (share == null)
            {
                return NotFound();
            }

            _context.Shares.Remove(share);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ShareExists(int id)
        {
            return _context.Shares.Any(e => e.ShareId == id);
        }
    }
}
