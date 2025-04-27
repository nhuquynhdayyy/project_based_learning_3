using Microsoft.EntityFrameworkCore;
using TourismWeb.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Logging.Console;

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

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("TourismDB")));

// 🔹 Đăng ký Authentication (nếu dùng Cookie Auth)
// builder.Services.AddAuthentication("CookieAuth")
//     .AddCookie("CookieAuth", options =>
//     {
//         options.LoginPath = "/Users/Login";
//         options.LogoutPath = "/Users/Logout";
//     });
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Users/Login";     // Đường dẫn khi chưa đăng nhập
        options.LogoutPath = "/Users/Logout";   // Đường dẫn khi đăng xuất
        options.AccessDeniedPath = "/Home/AccessDenied"; // Đường dẫn khi bị từ chối truy cập
        options.Cookie.Name = "UserAuthCookie"; // Tên cookie
        options.Cookie.HttpOnly = true;         // Bảo vệ cookie khỏi truy cập từ JavaScript
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Thời gian hết hạn
        options.SlidingExpiration = true;       // Gia hạn tự động khi còn hoạt động
    });

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Logging.ClearProviders();
builder.Logging.AddConsole(options =>
{
    options.FormatterName = ConsoleFormatterNames.Systemd; // Hoặc dùng JsonConsole
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession(); // Bật session
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
    endpoints.MapControllers();
});

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

TestConnection.Run();

app.Run();
