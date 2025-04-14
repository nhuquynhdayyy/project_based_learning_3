using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult ManageUsers()
    {
        return View();
    }

    public IActionResult Dashboard()
    {
        return View();
    }
    public IActionResult Posts()
    {
        return View();
    }
    public IActionResult Comments()
    {
        return View();
    }
    public IActionResult Interactions()
    {
        return View();
    }
    public IActionResult Reports()
    {
        return View();
    }
    public IActionResult Statistics()
    {
        return View();
    }
    public IActionResult Users()
    {
        return View();
    }
    public IActionResult Settings()
    {
        return View();
    }
}
