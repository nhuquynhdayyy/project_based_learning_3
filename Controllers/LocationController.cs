using Microsoft.AspNetCore.Mvc;

namespace TourismWeb.Controllers
{
    public class LocationController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        // Action để hiển thị trang Đà Nẵng
        public IActionResult Danang1()
        {
            return View();
        }
        public IActionResult Hue1()
        {
            return View();
        }
        public IActionResult Hoian1()
        {
            return View();
        }
        public IActionResult Quangbinh1()
        {
            return View();
        }
        public IActionResult Nhatrang1()
        {
            return View();
        }
        public IActionResult Phuyen1()
        {
            return View();
        }
    }
}
