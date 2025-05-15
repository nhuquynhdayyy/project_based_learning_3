using Microsoft.EntityFrameworkCore;
using TourismWeb.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Logging.Console;
using TourismWeb.Services; // <<--- THÊM USING CHO SERVICE CỦA BẠN

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Thời gian hết hạn session
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Thêm dòng này để đăng ký IHttpContextAccessor
builder.Services.AddHttpContextAccessor();

builder.Services.AddDistributedMemoryCache();

DotNetEnv.Env.Load();
// Add services to the container.
builder.Services.AddControllersWithViews();
// THÊM DÒNG NÀY ĐỂ ĐĂNG KÝ IHttpClientFactory
builder.Services.AddHttpClient();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("TourismDB")));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Accounts/Login";     // ĐỔI Account THÀNH Accounts
        options.LogoutPath = "/Accounts/Logout";   // ĐỔI Accout THÀNH Accounts
        options.AccessDeniedPath = "/Home/AccessDenied"; // Đường dẫn khi bị từ chối truy cập
        options.Cookie.Name = "UserAuthCookie"; // Tên cookie
        options.Cookie.HttpOnly = true;         // Bảo vệ cookie khỏi truy cập từ JavaScript
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Thời gian hết hạn
        options.SlidingExpiration = true;       // Gia hạn tự động khi còn hoạt động
    });

// =======================================================
// THÊM DÒNG ĐĂNG KÝ IEmailSender VÀ SmtpEmailSender Ở ĐÂY
// =======================================================
builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));

builder.Logging.ClearProviders();
builder.Logging.AddConsole(options =>
{
    // options.FormatterName = ConsoleFormatterNames.Systemd; // Dòng này có thể gây lỗi nếu không có package tương ứng
    // Hãy thử comment nó đi nếu gặp lỗi khi chạy
    // Hoặc đảm bảo bạn đã cài Microsoft.Extensions.Logging.Console (thường có sẵn)
    // và SystemdConsoleFormatter nếu bạn thực sự muốn dùng định dạng đó.
    // Mặc định AddConsole() đã đủ dùng.
});


var app = builder.Build();

// var apiKey = Environment.GetEnvironmentVariable("OPENWEATHERMAP_API_KEY"); // Dòng này nên nằm trong một service hoặc controller nếu cần
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "admin",
        pattern: "Admin/{action=Dashboard}/{id?}",
        defaults: new { controller = "Admin" });

    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
    // endpoints.MapControllers(); // Dòng này không cần thiết nếu bạn đã dùng MapControllerRoute
});

// app.MapStaticAssets(); // Dòng này có thể không cần thiết, UseStaticFiles() đã xử lý

// app.MapControllerRoute( // Bạn đã có MapControllerRoute "default" ở trên rồi, dòng này lặp lại
//     name: "default",
//     pattern: "{controller=Home}/{action=Index}/{id?}")
//     .WithStaticAssets();

// TestConnection.Run(); // Dòng này để làm gì? Nếu là test, nên có điều kiện hoặc gỡ bỏ khi deploy

app.Run();