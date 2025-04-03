using Microsoft.AspNetCore.Mvc;

namespace TourismWeb.Controllers
{
    public class LocationController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
