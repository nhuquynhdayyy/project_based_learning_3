// ReportController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;   
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using System.Threading.Tasks;
using TourismWeb.Models;
namespace TourismWeb.Controllers
{
    [Authorize] // Phải đăng nhập mới được báo cáo
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        // [HttpGet]
        // public IActionResult Create(string targetType, int targetId, int? reportedUserId)
        // {
        //     ViewBag.TargetType = targetType;
        //     ViewBag.TargetId = targetId;
        //     ViewBag.ReportedUserId = reportedUserId;
        //     return View();
        // }
        [HttpGet]
public IActionResult Create(string targetType, int targetId, int? reportedUserId)
{
    // ✅ Parse string thành enum
    if (!Enum.TryParse<ReportTargetType>(targetType, out var parsedTargetType))
    {
        ModelState.AddModelError("TargetType", "Loại mục tiêu không hợp lệ.");
        return View();
    }

    var report = new Report
    {
        TargetType = parsedTargetType,
        TargetId = targetId,
        ReportedUserId = reportedUserId
    };
    Console.WriteLine("TargetType: " + report.TargetType);

    return View(report);
}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Report report)
        {
            if (!ModelState.IsValid)
    {
        // Debug lỗi
        foreach (var value in ModelState.Values)
        {
            foreach (var error in value.Errors)
            {
                Console.WriteLine("Model Error: " + error.ErrorMessage);
            }
        }
        return View(report);
    }
            if (ModelState.IsValid)
            {
                report.ReportedAt = DateTime.Now;
                report.Status = ReportStatus.Pending;
//                 var userIdClaim = User.FindFirst("UserId");
// if (userIdClaim == null)
// {
//     ModelState.AddModelError(string.Empty, "Không xác định được người dùng hiện tại.");
//     return View(report);
// }

// report.ReporterUserId = int.Parse(userIdClaim.Value);
                report.ReporterUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                // report.ReporterUserId = int.Parse(User.FindFirst("UserId").Value); // lấy ID người dùng từ session hoặc claims
                ViewBag.ReportTypes = new SelectList(Enum.GetValues(typeof(ReportType)).Cast<ReportType>());
                ViewBag.TargetTypes = new SelectList(Enum.GetValues(typeof(ReportTargetType)).Cast<ReportTargetType>());
                _context.Reports.Add(report);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Báo cáo của bạn đã được gửi. Cảm ơn bạn!";
                return RedirectToAction("Index", "Home");
            }
            return View(report);
        }
    }
}