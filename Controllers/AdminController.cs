using Microsoft.AspNetCore.Authorization;
// Trong AdminController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourismWeb.Models; // Namespace chứa ApplicationDbContext và các Model
using System.Linq;
using System.Threading.Tasks;
using System.Globalization; // Cho định dạng ngày tháng
using System.Text.Json;
using System.Collections.Generic; // Required for List
using System.Security.Claims;
using Microsoft.Extensions.Configuration;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AdminController> _logger; // Thêm logger vào đây nếu cần thiết
     private readonly IConfiguration _configuration;

    
    [ActivatorUtilitiesConstructor]
    public AdminController(ApplicationDbContext context, ILogger<AdminController> logger, IConfiguration configuration)
    {
        _context = context;
        _logger = logger; // Khởi tạo logger
          _configuration = configuration;
    }

    private int GetCurrentAdminUserId()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(userIdString, out int userId))
        {
            return userId;
        }
        throw new Exception("Không thể xác định User ID của admin.");
    }
    
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


    
     // GET: /Admin/Settings
    public async Task<IActionResult> Settings()
    {
        int adminUserId = GetCurrentAdminUserId();
        var adminUser = await _context.Users.FindAsync(adminUserId);

        if (adminUser == null)
        {
            return NotFound("Không tìm thấy tài khoản admin.");
        }

        Dictionary<string, object?> settings = new Dictionary<string, object?>();

        if (!string.IsNullOrEmpty(adminUser.Bio))
        {
            try
            {
                var parsedJson = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(adminUser.Bio);
                if (parsedJson != null)
                {
                    foreach (var item in parsedJson)
                    {
                        switch (item.Value.ValueKind)
                        {
                            case JsonValueKind.String:
                                settings[item.Key] = item.Value.GetString();
                                break;
                            case JsonValueKind.Number:
                                if (item.Value.TryGetInt32(out int intVal)) settings[item.Key] = intVal;
                                else if (item.Value.TryGetDouble(out double dblVal)) settings[item.Key] = dblVal;
                                else settings[item.Key] = item.Value.GetRawText();
                                break;
                            case JsonValueKind.True:
                                settings[item.Key] = true;
                                break;
                            case JsonValueKind.False:
                                settings[item.Key] = false;
                                break;
                            case JsonValueKind.Null:
                                settings[item.Key] = null;
                                break;
                        }
                    }
                }
            }
            catch (JsonException)
            {
                // ModelState.AddModelError("Bio", "Dữ liệu cài đặt trong Bio không hợp lệ.");
            }
        }

        // Gán giá trị từ JSON hoặc mặc định vào ViewData
        ViewData["SiteName"] = settings.GetValueOrDefault("SiteName", "Tên Trang Web Mặc Định");
        ViewData["SiteDescription"] = settings.GetValueOrDefault("SiteDescription", "Mô tả mặc định.");
        // URL trang web sẽ lấy từ config, không từ Bio nữa
        ViewData["SiteUrl_ReadOnly"] = _configuration["AppSettings:SiteUrl"] ?? "https://chua-cau-hinh.com";

        ViewData["AdminPanelLanguage"] = settings.GetValueOrDefault("AdminPanelLanguage", "vi");
        ViewData["Timezone"] = settings.GetValueOrDefault("Timezone", "Asia/Ho_Chi_Minh");
        ViewData["DateFormat"] = settings.GetValueOrDefault("DateFormat", "dd/MM/yyyy");
        ViewData["PostsPerPage"] = settings.GetValueOrDefault("PostsPerPage", 10);
        ViewData["CommentsPerPage"] = settings.GetValueOrDefault("CommentsPerPage", 10);
        ViewData["DefaultCategoryId"] = settings.GetValueOrDefault("DefaultCategoryId", null);
        ViewData["AutoApproveCommentsRegisteredUsers"] = settings.GetValueOrDefault("AutoApproveCommentsRegisteredUsers", true);
        ViewData["AdminTheme"] = settings.GetValueOrDefault("AdminTheme", "system");
        ViewData["AdminPrimaryColor"] = settings.GetValueOrDefault("AdminPrimaryColor", "#3B82F6");
        ViewData["AdminFontFamily"] = settings.GetValueOrDefault("AdminFontFamily", "Roboto, sans-serif");
        ViewData["NotifyInAppNewComment"] = settings.GetValueOrDefault("NotifyInAppNewComment", true);
        ViewData["NotifyEmailNewUser"] = settings.GetValueOrDefault("NotifyEmailNewUser", false);
        ViewData["RequireCommentApprovalGlobal"] = settings.GetValueOrDefault("RequireCommentApprovalGlobal", true);
        ViewData["RequireEmailVerification"] = settings.GetValueOrDefault("RequireEmailVerification", true);
        ViewData["SystemVersion"] = settings.GetValueOrDefault("SystemVersion", "1.0.0");

        // Không cần truyền ID bài viết chính sách/điều khoản nữa nếu chúng là trang tĩnh
        // ViewBag.Categories = await _context.Categories.ToListAsync();

        return View("~/Views/Admin/Settings.cshtml");
    }

    // POST: /Admin/SaveSettings
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveSettings(IFormCollection form)
    {
        int adminUserId = GetCurrentAdminUserId();
        var adminUser = await _context.Users.FindAsync(adminUserId);

        if (adminUser == null)
        {
            return NotFound("Không tìm thấy tài khoản admin.");
        }

        var settingsToSave = new Dictionary<string, object?>();

        settingsToSave["SiteName"] = form["SiteName"].ToString();
        settingsToSave["SiteDescription"] = form["SiteDescription"].ToString();
        // KHÔNG LƯU SiteUrl từ form, nó được đọc từ config
        // settingsToSave["SiteUrl"] = form["SiteUrl"].ToString(); // Bỏ dòng này

        settingsToSave["AdminPanelLanguage"] = form["AdminPanelLanguage"].ToString();
        settingsToSave["Timezone"] = form["Timezone"].ToString();
        settingsToSave["DateFormat"] = form["DateFormat"].ToString();

        if (int.TryParse(form["PostsPerPage"], out int postsPerPage))
            settingsToSave["PostsPerPage"] = postsPerPage;
        else
            settingsToSave["PostsPerPage"] = 10;

        if (int.TryParse(form["CommentsPerPage"], out int commentsPerPage))
            settingsToSave["CommentsPerPage"] = commentsPerPage;
        else
            settingsToSave["CommentsPerPage"] = 10;

        if (int.TryParse(form["DefaultCategoryId"], out int defaultCategoryId) && defaultCategoryId > 0)
            settingsToSave["DefaultCategoryId"] = defaultCategoryId;
        else
            settingsToSave["DefaultCategoryId"] = null;

        settingsToSave["AutoApproveCommentsRegisteredUsers"] = form.ContainsKey("AutoApproveCommentsRegisteredUsers") &&
            (form["AutoApproveCommentsRegisteredUsers"].ToString().ToLower() == "true" || form["AutoApproveCommentsRegisteredUsers"].ToString().ToLower() == "on");

        settingsToSave["AdminTheme"] = form["AdminTheme"].ToString();
        settingsToSave["AdminPrimaryColor"] = form["AdminPrimaryColor"].ToString();
        settingsToSave["AdminFontFamily"] = form["AdminFontFamily"].ToString();

        settingsToSave["NotifyInAppNewComment"] = form.ContainsKey("NotifyInAppNewComment") && (form["NotifyInAppNewComment"].ToString().ToLower() == "true" || form["NotifyInAppNewComment"].ToString().ToLower() == "on");
        settingsToSave["NotifyEmailNewUser"] = form.ContainsKey("NotifyEmailNewUser") && (form["NotifyEmailNewUser"].ToString().ToLower() == "true" || form["NotifyEmailNewUser"].ToString().ToLower() == "on");

        settingsToSave["RequireCommentApprovalGlobal"] = form.ContainsKey("RequireCommentApprovalGlobal") && (form["RequireCommentApprovalGlobal"].ToString().ToLower() == "true" || form["RequireCommentApprovalGlobal"].ToString().ToLower() == "on");
        settingsToSave["RequireEmailVerification"] = form.ContainsKey("RequireEmailVerification") && (form["RequireEmailVerification"].ToString().ToLower() == "true" || form["RequireEmailVerification"].ToString().ToLower() == "on");

        // KHÔNG LƯU ID bài viết chính sách/điều khoản từ form này nữa
        // if (int.TryParse(form["PrivacyPolicyPostId"], out int privacyId) && privacyId != 0)
        //     settingsToSave["PrivacyPolicyPostId"] = privacyId;
        // else
        //     settingsToSave["PrivacyPolicyPostId"] = null;

        // if (int.TryParse(form["TermsOfServicePostId"], out int termsId) && termsId != 0)
        //     settingsToSave["TermsOfServicePostId"] = termsId;
        // else
        //     settingsToSave["TermsOfServicePostId"] = null;

        settingsToSave["SystemVersion"] = form["SystemVersion"].ToString();

        adminUser.Bio = JsonSerializer.Serialize(settingsToSave, new JsonSerializerOptions { WriteIndented = true });

        try
        {
            _context.Users.Update(adminUser);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Cài đặt đã được lưu thành công!";
        }
        catch (DbUpdateException ex)
        {
            TempData["ErrorMessage"] = $"Lỗi khi lưu cài đặt: {ex.Message}";
        }

        return RedirectToAction("Settings");
    }
}
