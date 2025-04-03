using Microsoft.AspNetCore.Mvc;

namespace TourismWeb.Controllers
{
    public class GuideController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
