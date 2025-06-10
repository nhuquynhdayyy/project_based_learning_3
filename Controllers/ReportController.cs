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
    [Authorize] 
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public IActionResult Create(string targetType, int targetId, int? reportedUserId)
        {
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
                report.ReporterUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                ViewBag.ReportTypes = new SelectList(Enum.GetValues(typeof(ReportType)).Cast<ReportType>());
                ViewBag.TargetTypes = new SelectList(Enum.GetValues(typeof(ReportTargetType)).Cast<ReportTargetType>());
                _context.Reports.Add(report);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Báo cáo của bạn đã được gửi. Cảm ơn bạn!";
                return RedirectToAction("Index", "Home");
            }
            return View(report);
        }

        // --- MÃ NGUỒN MỚI THÊM VÀO ---

        // GET: Report
        // Hiển thị danh sách tất cả báo cáo cho Admin
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var reports = await _context.Reports
                                .Include(r => r.ReporterUser) // Lấy thông tin người báo cáo
                                .Include(r => r.ReportedUser) // Lấy thông tin người bị báo cáo
                                .OrderByDescending(r => r.ReportedAt)
                                .ToListAsync();
            return View(reports);
        }

        // GET: Report/Delete/5
        // Hiển thị trang xác nhận xóa
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var report = await _context.Reports
                .Include(r => r.ReporterUser)
                .FirstOrDefaultAsync(m => m.ReportId == id);
            
            if (report == null)
            {
                return NotFound();
            }

            return View(report);
        }

        // POST: Report/Delete/5
        // Thực hiện xóa sau khi Admin xác nhận
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report != null)
            {
                _context.Reports.Remove(report);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Báo cáo đã được xóa thành công.";
            }
            
            return RedirectToAction(nameof(Index));
        }
    }
}