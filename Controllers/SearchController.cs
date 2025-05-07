using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TourismWeb.Models;
using TourismWeb.Models.ViewModels;

namespace TourismWeb.Controllers
{
    public class SearchController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SearchController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return View(new SearchResultViewModel());
            }

            var searchTermLower = searchTerm.ToLower();

            // Tìm kiếm posts theo tiêu đề
            var posts = await _context.Posts
                .Where(p => p.Title.ToLower().Contains(searchTermLower))
                .Include(p => p.Spot)
                .Include(p => p.User)
                .ToListAsync();

            // Tìm kiếm tourist spots theo tên
            var touristSpots = await _context.TouristSpots
                .Where(t => t.Name.ToLower().Contains(searchTermLower))
                .Include(t => t.Category)
                .ToListAsync();

            var viewModel = new SearchResultViewModel
            {
                SearchTerm = searchTerm,
                Posts = posts,
                TouristSpots = touristSpots
            };

            return View(viewModel);
        }
    }
}