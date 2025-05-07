using Microsoft.AspNetCore.Mvc;

namespace TourismWeb.Controllers
{
    public class ContactController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
