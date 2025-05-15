// // using Microsoft.AspNetCore.Mvc;
// // using Microsoft.EntityFrameworkCore;
// // using System.Security.Claims;
// // using Microsoft.AspNetCore.Authentication;
// // using Microsoft.AspNetCore.Authentication.Cookies;
// // using TourismWeb.Models; // Đảm bảo User model ở đây
// // using System.Threading.Tasks;
// // using System.Linq;
// // using System.Collections.Generic;
// // using System;
// // using Microsoft.Extensions.Configuration; // Để đọc AppId, AppSecret
// // using System.Net.Http; // Cho HttpClient
// // using Newtonsoft.Json.Linq; // Cho việc parse JSON response từ Facebook
// // using System.ComponentModel.DataAnnotations; // Cho các ViewModel
// // using Microsoft.Extensions.Logging; // Thêm để sử dụng ILogger (tùy chọn nâng cao)
// // using Google.Apis.Auth;

// // namespace TourismWeb.Controllers
// // {
// //     // Model để nhận dữ liệu từ client khi đăng nhập bằng Facebook
// //     public class FacebookLoginViewModel
// //     {
// //         [Required]
// //         public string FacebookUserId { get; set; } = null!;
// //         public string? Email { get; set; } // Email có thể null
// //         public string? FullName { get; set; } // FullName có thể null
// //         [Required]
// //         public string AccessToken { get; set; } = null!;
// //     }

// //     // Model để nhận ID Token từ client khi đăng nhập bằng Google
// //     public class GoogleLoginViewModel
// //     {
// //         [Required]
// //         public string IdToken { get; set; } = null!;
// //     }

// //     public class AccountsController : Controller
// //     {
// //         private readonly ApplicationDbContext _context;
// //         private readonly IConfiguration _configuration;
// //         private readonly IHttpClientFactory _httpClientFactory;
// //         // private readonly ILogger<AccountsController> _logger;

// //         public AccountsController(ApplicationDbContext context,
// //                                   IConfiguration configuration,
// //                                   IHttpClientFactory httpClientFactory /*, ILogger<AccountsController> logger */)
// //         {
// //             _context = context;
// //             _configuration = configuration;
// //             _httpClientFactory = httpClientFactory;
// //             // _logger = logger;
// //         }

// //         // GET: Accounts/Register
// //         public IActionResult Register()
// //         {
// //             Console.WriteLine("AccountsController: Register GET action called.");
// //             return View();
// //         }

// //         // POST: Accounts/Register
// //         [HttpPost]
// //         [ValidateAntiForgeryToken]
// //         public async Task<IActionResult> Register(User model, string ConfirmPassword)
// //         {
// //             Console.WriteLine("AccountsController: Register POST action called.");
// //             if (ModelState.IsValid)
// //             {
// //                 Console.WriteLine("Register POST: ModelState is Valid.");
// //                 if (model.Password != ConfirmPassword)
// //                 {
// //                     ViewBag.ErrorMessage = "Mật khẩu xác nhận không khớp!";
// //                     Console.WriteLine("Register POST Error: Passwords do not match.");
// //                     return View(model);
// //                 }

// //                 if (await _context.Users.AnyAsync(u => u.Username == model.Username))
// //                 {
// //                     ViewBag.ErrorMessage = "Tên đăng nhập đã tồn tại!";
// //                     Console.WriteLine($"Register POST Error: Username '{model.Username}' already exists.");
// //                     return View(model);
// //                 }
// //                 if (await _context.Users.AnyAsync(u => u.Email == model.Email))
// //                 {
// //                     ViewBag.ErrorMessage = "Email đã tồn tại!";
// //                     Console.WriteLine($"Register POST Error: Email '{model.Email}' already exists.");
// //                     return View(model);
// //                 }

// //                 model.CreatedAt = DateTime.Now;
// //                 model.Role = "User";
// //                 // QUAN TRỌNG: Hash mật khẩu trước khi lưu nếu đây là đăng ký thường
// //                 // model.Password = HashPasswordSomehow(model.Password);

// //                 try
// //                 {
// //                     _context.Users.Add(model);
// //                     await _context.SaveChangesAsync();
// //                     Console.WriteLine($"Register POST: User '{model.Username}' created successfully.");

// //                     await SignInUser(model, false);
// //                     Console.WriteLine($"Register POST: User '{model.Username}' signed in.");
// //                     return RedirectToAction("Index", "Home");
// //                 }
// //                 catch (DbUpdateException dbEx)
// //                 {
// //                     Console.WriteLine($"Register POST DbUpdateException: {dbEx.ToString()}");
// //                     string innerErrorMessages = GetInnerExceptionMessages(dbEx);
// //                     ViewBag.ErrorMessage = "Lỗi khi lưu dữ liệu vào database (Register). Chi tiết: " + innerErrorMessages;
// //                     Console.WriteLine("Register POST Inner Exception Details: " + innerErrorMessages);
// //                     return View(model);
// //                 }
// //                 catch (Exception ex)
// //                 {
// //                     ViewBag.ErrorMessage = "Lỗi lưu dữ liệu không xác định. Vui lòng thử lại.";
// //                     Console.WriteLine($"Register POST General Exception: {ex.ToString()}");
// //                     return View(model);
// //                 }
// //             }
// //             else
// //             {
// //                 var errors = ModelState.Values.SelectMany(v => v.Errors)
// //                                             .Select(e => e.ErrorMessage)
// //                                             .ToList();
// //                 ViewBag.ErrorMessage = string.Join("<br>", errors);
// //                 Console.WriteLine($"Register POST Error: ModelState is Invalid. Errors: {ViewBag.ErrorMessage}");
// //                 return View(model);
// //             }
// //         }


// //         // GET: Accounts/Login
// //         public IActionResult Login(string? returnUrl = null)
// //         {
// //             Console.WriteLine($"AccountsController: Login GET action called. ReturnUrl: {returnUrl}");
// //             ViewData["ReturnUrl"] = returnUrl;
// //             return View();
// //         }

// //         // POST: Accounts/Login
// //         [HttpPost]
// //         [ValidateAntiForgeryToken]
// //         public async Task<IActionResult> Login(string UsernameOrEmail, string Password, bool RememberMe, string? returnUrl = null)
// //         {
// //             Console.WriteLine($"AccountsController: Login POST action called. UsernameOrEmail: {UsernameOrEmail}, RememberMe: {RememberMe}, ReturnUrl: {returnUrl}");
// //             ViewData["ReturnUrl"] = returnUrl;
// //             if (string.IsNullOrEmpty(UsernameOrEmail) || string.IsNullOrEmpty(Password))
// //             {
// //                 ViewBag.ErrorMessage = "Vui lòng nhập đầy đủ thông tin!";
// //                 Console.WriteLine("Login POST Error: Username/Email or Password is empty.");
// //                 return View();
// //             }

// //             // QUAN TRỌNG: So sánh hash mật khẩu, không phải mật khẩu gốc
// //             var user = await _context.Users
// //                 .FirstOrDefaultAsync(u => (u.Username == UsernameOrEmail || u.Email == UsernameOrEmail) && u.Password == Password /* THAY BẰNG SO SÁNH HASH */);

// //             if (user == null)
// //             {
// //                 ViewBag.ErrorMessage = "Tên đăng nhập, email hoặc mật khẩu không chính xác!";
// //                 Console.WriteLine("Login POST Error: Invalid username, email, or password.");
// //                 return View();
// //             }

// //             Console.WriteLine($"Login POST: User '{user.Username}' found.");
// //             await SignInUser(user, RememberMe);
// //             Console.WriteLine($"Login POST: User '{user.Username}' signed in.");

// //             if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
// //             {
// //                 Console.WriteLine($"Login POST: Redirecting to local ReturnUrl: {returnUrl}");
// //                 return Redirect(returnUrl);
// //             }
// //             Console.WriteLine("Login POST: Redirecting to Home/Index.");
// //             return RedirectToAction("Index", "Home");
// //         }

// //         // POST: Accounts/FacebookLogin
// //         [HttpPost]
// //         [ValidateAntiForgeryToken]
// //         public async Task<IActionResult> FacebookLogin(FacebookLoginViewModel model, string? returnUrl = null)
// //         {
// //             Console.WriteLine("AccountsController: FacebookLogin POST action called.");
// //             Console.WriteLine($"FacebookLogin POST - Received Model: UserId='{model.FacebookUserId}', Email='{model.Email}', FullName='{model.FullName}', AccessToken IS {(string.IsNullOrEmpty(model.AccessToken) ? "EMPTY" : "PRESENT")}");
// //             ViewData["ReturnUrl"] = returnUrl;

// //             if (!ModelState.IsValid)
// //             {
// //                 var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
// //                 TempData["FacebookLoginError"] = "Dữ liệu đăng nhập Facebook không hợp lệ (ModelState): " + string.Join("; ", errors);
// //                 Console.WriteLine($"FacebookLogin POST Error: ModelState is Invalid. Errors: {string.Join("; ", errors)}");
// //                 return RedirectToAction("Login", new { returnUrl });
// //             }
// //             Console.WriteLine("FacebookLogin POST: ModelState is Valid.");

// //             // --- Bước 1: Xác thực AccessToken với Facebook Server-side ---
// //             var facebookAppId = _configuration["Authentication:Facebook:AppId"];
// //             var facebookAppSecret = _configuration["Authentication:Facebook:AppSecret"];
// //             Console.WriteLine($"FacebookLogin POST - Config: AppId='{facebookAppId}', AppSecret IS {(string.IsNullOrEmpty(facebookAppSecret) ? "EMPTY" : "PRESENT")}");


// //             if (string.IsNullOrEmpty(facebookAppId) || string.IsNullOrEmpty(facebookAppSecret))
// //             {
// //                 TempData["FacebookLoginError"] = "Cấu hình Facebook App ID/App Secret bị thiếu trên server.";
// //                 Console.WriteLine("FacebookLogin POST Error: Facebook AppId or AppSecret is missing in configuration.");
// //                 return RedirectToAction("Login", new { returnUrl });
// //             }

// //             var httpClient = _httpClientFactory.CreateClient();
// //             var debugTokenUrl = $"https://graph.facebook.com/debug_token?input_token={model.AccessToken}&access_token={facebookAppId}|{facebookAppSecret}";
// //             Console.WriteLine($"FacebookLogin POST - Debug Token URL: {debugTokenUrl}");

// //             try
// //             {
// //                 Console.WriteLine("FacebookLogin POST: Attempting to call Facebook debug_token API.");
// //                 var response = await httpClient.GetAsync(debugTokenUrl);
// //                 Console.WriteLine($"FacebookLogin POST - Debug Token API Response Status: {response.StatusCode}");

// //                 if (!response.IsSuccessStatusCode)
// //                 {
// //                     var errorContent = await response.Content.ReadAsStringAsync();
// //                     TempData["FacebookLoginError"] = $"Không thể xác thực token với Facebook. Status: {response.StatusCode}. Phản hồi từ FB: {errorContent}";
// //                     Console.WriteLine($"FacebookLogin POST Error: Failed to validate token with Facebook. Status: {response.StatusCode}. FB Response: {errorContent}");
// //                     return RedirectToAction("Login", new { returnUrl });
// //                 }

// //                 var content = await response.Content.ReadAsStringAsync();
// //                 Console.WriteLine($"FacebookLogin POST - Debug Token API Success Response Content: {content}");
// //                 var tokenValidationResult = JObject.Parse(content);
// //                 var tokenData = tokenValidationResult["data"];

// //                 if (tokenData == null)
// //                 {
// //                     TempData["FacebookLoginError"] = "Token Facebook không hợp lệ: không có trường 'data' trong phản hồi.";
// //                     Console.WriteLine($"FacebookLogin POST Error: Invalid Facebook token - 'data' field missing in response. Full response: {content}");
// //                     return RedirectToAction("Login", new { returnUrl });
// //                 }

// //                 bool isValid = tokenData["is_valid"]?.Value<bool>() ?? false;
// //                 string? appIdFromToken = tokenData["app_id"]?.Value<string>();
// //                 string? userIdFromToken = tokenData["user_id"]?.Value<string>();

// //                 Console.WriteLine($"FacebookLogin POST - Token Validation: is_valid='{isValid}', app_id_from_token='{appIdFromToken}', user_id_from_token='{userIdFromToken}'");

// //                 if (!isValid || appIdFromToken != facebookAppId || userIdFromToken != model.FacebookUserId)
// //                 {
// //                     TempData["FacebookLoginError"] = "Token Facebook không hợp lệ hoặc không khớp với ứng dụng/người dùng.";
// //                     if (!isValid) Console.WriteLine("FacebookLogin POST Error: Token is_valid is false.");
// //                     if (appIdFromToken != facebookAppId) Console.WriteLine($"FacebookLogin POST Error: AppId mismatch. Token AppId: {appIdFromToken}, Config AppId: {facebookAppId}.");
// //                     if (userIdFromToken != model.FacebookUserId) Console.WriteLine($"FacebookLogin POST Error: UserId mismatch. Token UserId: {userIdFromToken}, Model UserId: {model.FacebookUserId}.");
// //                     return RedirectToAction("Login", new { returnUrl });
// //                 }
// //                 Console.WriteLine("FacebookLogin POST: Token validation successful.");
// //             }
// //             catch (Exception ex)
// //             {
// //                 TempData["FacebookLoginError"] = "Lỗi khi xác thực token Facebook với server: " + ex.Message;
// //                 Console.WriteLine($"FacebookLogin POST Exception during token validation: {ex.ToString()}");
// //                 return RedirectToAction("Login", new { returnUrl });
// //             }
// //             // --- Kết thúc Bước 1 ---

// //             try
// //             {
// //                 Console.WriteLine("FacebookLogin POST: Attempting to find or create user.");
// //                 var user = await _context.Users.FirstOrDefaultAsync(u => u.FacebookId == model.FacebookUserId);

// //                 if (user != null)
// //                 {
// //                     Console.WriteLine($"FacebookLogin POST: User found with FacebookId '{model.FacebookUserId}'. Username: '{user.Username}'.");
// //                     await SignInUser(user, false);
// //                     Console.WriteLine($"FacebookLogin POST: Existing user '{user.Username}' signed in.");
// //                     if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
// //                     {
// //                         Console.WriteLine($"FacebookLogin POST: Redirecting existing user to local ReturnUrl: {returnUrl}");
// //                         return Redirect(returnUrl);
// //                     }
// //                     Console.WriteLine("FacebookLogin POST: Redirecting existing user to Home/Index.");
// //                     return RedirectToAction("Index", "Home");
// //                 }
// //                 else
// //                 {
// //                     Console.WriteLine($"FacebookLogin POST: No user found with FacebookId '{model.FacebookUserId}'. Attempting to create new user.");
// //                     if (!string.IsNullOrEmpty(model.Email))
// //                     {
// //                         Console.WriteLine($"FacebookLogin POST: Checking if email '{model.Email}' already exists for another user.");
// //                         var existingUserWithEmail = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
// //                         if (existingUserWithEmail != null)
// //                         {
// //                             TempData["FacebookLoginError"] = $"Đã tồn tại tài khoản với email {model.Email}. Vui lòng đăng nhập bằng tài khoản đó và liên kết Facebook từ trang cá nhân (nếu muốn).";
// //                             Console.WriteLine($"FacebookLogin POST Error: Email '{model.Email}' already exists for user '{existingUserWithEmail.Username}'.");
// //                             return RedirectToAction("Login", new { returnUrl });
// //                         }
// //                         Console.WriteLine($"FacebookLogin POST: Email '{model.Email}' is available or not provided by Facebook.");
// //                     }

// //                     var newUsername = !string.IsNullOrEmpty(model.Email) ? model.Email : $"fb_{model.FacebookUserId}";
// //                     if (await _context.Users.AnyAsync(u => u.Username == newUsername))
// //                     {
// //                         Console.WriteLine($"FacebookLogin POST: Generated username '{newUsername}' already exists. Generating a unique one.");
// //                         newUsername = $"fb_{model.FacebookUserId}_{Guid.NewGuid().ToString("N").Substring(0, 6)}";
// //                     }
// //                     Console.WriteLine($"FacebookLogin POST: Final username for new user: '{newUsername}'.");

// //                     var newUser = new User
// //                     {
// //                         Username = newUsername,
// //                         Email = model.Email,
// //                         FullName = model.FullName ?? "Người dùng Facebook",
// //                         FacebookId = model.FacebookUserId,
// //                         Role = "User",
// //                         CreatedAt = DateTime.Now,
// //                         Password = GenerateRandomPasswordPlaceholder()
// //                     };

// //                     _context.Users.Add(newUser);
// //                     Console.WriteLine($"FacebookLogin POST: Attempting to save new user '{newUser.Username}' (UserId before save: {newUser.UserId}). Details: Email='{newUser.Email}', FullName='{newUser.FullName}', FbId='{newUser.FacebookId}', PasswordPlaceholderSet=True");
// //                     await _context.SaveChangesAsync();
// //                     Console.WriteLine($"FacebookLogin POST: New user '{newUser.Username}' created successfully with UserId '{newUser.UserId}'.");

// //                     await SignInUser(newUser, false);
// //                     Console.WriteLine($"FacebookLogin POST: New user '{newUser.Username}' signed in.");
// //                     if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
// //                     {
// //                         Console.WriteLine($"FacebookLogin POST: Redirecting new user to local ReturnUrl: {returnUrl}");
// //                         return Redirect(returnUrl);
// //                     }
// //                     Console.WriteLine("FacebookLogin POST: Redirecting new user to Home/Index.");
// //                     return RedirectToAction("Index", "Home");
// //                 }
// //             }
// //             catch (DbUpdateException dbEx)
// //             {
// //                 Console.WriteLine($"FacebookLogin POST DbUpdateException: {dbEx.ToString()}");
// //                 string innerErrorMessages = GetInnerExceptionMessages(dbEx);
// //                 TempData["FacebookLoginError"] = "Lỗi khi lưu dữ liệu người dùng Facebook vào database. Chi tiết: " + innerErrorMessages;
// //                 Console.WriteLine("FacebookLogin POST Inner Exception Details: " + innerErrorMessages);
// //                 return RedirectToAction("Login", new { returnUrl });
// //             }
// //             catch (Exception ex)
// //             {
// //                 TempData["FacebookLoginError"] = "Đã xảy ra lỗi không xác định trong quá trình xử lý đăng nhập Facebook: " + ex.Message;
// //                 Console.WriteLine($"FacebookLogin POST General Exception during user find/create: {ex.ToString()}");
// //                 return RedirectToAction("Login", new { returnUrl });
// //             }
// //         }


// //         // ============================================================
// //         // BẮT ĐẦU ACTION GOOGLE LOGIN - ĐÃ SỬA CHO CHẾ ĐỘ OFFLINE DEBUG
// //         // ============================================================
// //         [HttpPost]
// //         [ValidateAntiForgeryToken]
// //         public async Task<IActionResult> GoogleLogin(GoogleLoginViewModel model, string? returnUrl = null)
// //         {
// //             Console.WriteLine("AccountsController: GoogleLogin POST action called.");
// //             Console.WriteLine($"GoogleLogin POST - Received IdToken: {(string.IsNullOrEmpty(model.IdToken) ? "EMPTY" : "PRESENT (length: " + model.IdToken.Length + ")")}");
// //             ViewData["ReturnUrl"] = returnUrl;

// //             // CỜ ĐỂ BẬT/TẮT CHẾ ĐỘ DEBUG OFFLINE
// //             // !!! QUAN TRỌNG: ĐẶT THÀNH 'false' KHI DEPLOY LÊN PRODUCTION HOẶC KHI CÓ INTERNET !!!
// //             bool isOfflineDebugMode = true; // Đặt thành true nếu bạn đang test offline

// //             if (!ModelState.IsValid || string.IsNullOrEmpty(model.IdToken))
// //             {
// //                 TempData["GoogleLoginError"] = "Dữ liệu đăng nhập Google không hợp lệ (ID Token bị thiếu).";
// //                 Console.WriteLine("GoogleLogin POST Error: ModelState is Invalid or IdToken is missing.");
// //                 return RedirectToAction("Login", new { returnUrl });
// //             }

// //             try
// //             {
// //                 var googleClientId = _configuration["Authentication:Google:ClientId"];
// //                 if (string.IsNullOrEmpty(googleClientId))
// //                 {
// //                     TempData["GoogleLoginError"] = "Cấu hình Google Client ID bị thiếu trên server.";
// //                     Console.WriteLine("GoogleLogin POST Error: Google ClientID is missing in configuration.");
// //                     return RedirectToAction("Login", new { returnUrl });
// //                 }
// //                 Console.WriteLine($"GoogleLogin POST - Using Google Client ID from config: {googleClientId}");

// //                 GoogleJsonWebSignature.Payload payload; // Khai báo ở ngoài

// //                 if (isOfflineDebugMode)
// //                 {
// //                     Console.WriteLine("GoogleLogin POST WARNING: OFFLINE DEBUG MODE - SKIPPING ID TOKEN SIGNATURE VALIDATION. THIS IS NOT SAFE FOR PRODUCTION!");
// //                     try
// //                     {
// //                         var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
// //                         // Đọc token mà không validate chữ ký (TokenValidationParameters được thiết lập để bỏ qua validate chữ ký)
// //                         // CẢNH BÁO: Điều này KHÔNG an toàn cho production.
// //                         var jsonToken = handler.ReadJwtToken(model.IdToken);

// //                         // Trích xuất các claim cần thiết
// //                         payload = new GoogleJsonWebSignature.Payload // Tạo đối tượng Payload để chứa thông tin
// //                         {
// //                             Subject = jsonToken.Claims.FirstOrDefault(claim => claim.Type == "sub")?.Value,
// //                             Email = jsonToken.Claims.FirstOrDefault(claim => claim.Type == "email")?.Value,
// //                             Name = jsonToken.Claims.FirstOrDefault(claim => claim.Type == "name")?.Value,
// //                             Picture = jsonToken.Claims.FirstOrDefault(claim => claim.Type == "picture")?.Value,
// //                             EmailVerified = bool.TryParse(jsonToken.Claims.FirstOrDefault(claim => claim.Type == "email_verified")?.Value, out var ev) ? ev : false,
// //                             Audience = jsonToken.Audiences?.ToList() // Lấy danh sách audiences
// //                                                                      // Các claim khác nếu bạn cần, ví dụ:
// //                                                                      // Issuer = jsonToken.Issuer,
// //                                                                      // ExpirationTimeSeconds = jsonToken.ValidTo.ToUnixTimeSeconds(), // Unix time
// //                                                                      // IssuedAtTimeSeconds = jsonToken.ValidFrom.ToUnixTimeSeconds(), // Unix time
// //                         };

// //                         Console.WriteLine($"GoogleLogin POST (Offline Mode): Parsed Payload. Google User ID (sub): {payload.Subject}, Email: {payload.Email}, Name: {payload.Name}");

// //                         // Kiểm tra Audience (Client ID) thủ công
// //                         // Audience có thể là một chuỗi đơn hoặc một danh sách chuỗi
// //                         bool audienceMatch = false;
// //                         if (payload.Audience is string audString && audString == googleClientId)
// //                         {
// //                             audienceMatch = true;
// //                         }
// //                         else if (payload.Audience is IList<string> audList && audList.Contains(googleClientId))
// //                         {
// //                             audienceMatch = true;
// //                         }

// //                         if (!audienceMatch)
// //                         {
// //                             throw new InvalidJwtException($"Audience của ID Token không khớp với Client ID ('{googleClientId}') trong chế độ offline. Audience từ token: {string.Join(", ", jsonToken.Audiences ?? new List<string>())}");
// //                         }
// //                         Console.WriteLine("GoogleLogin POST (Offline Mode): Audience check passed.");
// //                     }
// //                     catch (Exception parseEx)
// //                     {
// //                         TempData["GoogleLoginError"] = "Lỗi parse ID Token (chế độ offline debug): " + parseEx.Message;
// //                         Console.WriteLine($"GoogleLogin POST Error: Failed to parse ID Token in offline debug mode. Exception: {parseEx.ToString()}");
// //                         return RedirectToAction("Login", new { returnUrl });
// //                     }
// //                 }
// //                 else // Chế độ online, validate bình thường
// //                 {
// //                     try
// //                     {
// //                         var validationSettings = new GoogleJsonWebSignature.ValidationSettings
// //                         {
// //                             Audience = new[] { googleClientId }
// //                             // Bạn có thể thêm Issuer nếu muốn: Issuers = new[] { "accounts.google.com", "https://accounts.google.com" }
// //                         };
// //                         Console.WriteLine("GoogleLogin POST: Attempting to validate Google ID Token online...");
// //                         payload = await GoogleJsonWebSignature.ValidateAsync(model.IdToken, validationSettings);
// //                         Console.WriteLine($"GoogleLogin POST: ID Token validated online. Google User ID (sub): {payload.Subject}, Email: {payload.Email}, Name: {payload.Name}, EmailVerified: {payload.EmailVerified}");
// //                     }
// //                     catch (Exception ex) // Bao gồm cả lỗi SSL nếu không có mạng
// //                     {
// //                         string errorMessage = "ID Token của Google không hợp lệ hoặc không thể xác thực: " + ex.Message;
// //                         Console.WriteLine($"GoogleLogin POST Error: Invalid Google ID Token (Online Mode). Main Exception: {ex.ToString()}");
// //                         if (ex.InnerException != null)
// //                         {
// //                             errorMessage += " | Inner Exception: " + ex.InnerException.Message;
// //                             Console.WriteLine($"GoogleLogin POST Error - INNER EXCEPTION (Online Mode): {ex.InnerException.ToString()}");
// //                         }
// //                         TempData["GoogleLoginError"] = errorMessage;
// //                         return RedirectToAction("Login", new { returnUrl });
// //                     }
// //                 }


// //                 // Từ đây, 'payload' đã có giá trị (hoặc đã validate online, hoặc chỉ parse offline)
// //                 var googleUserId = payload.Subject;
// //                 var email = payload.Email;
// //                 var fullName = payload.Name;
// //                 // var pictureUrl = payload.Picture;

// //                 if (string.IsNullOrEmpty(googleUserId))
// //                 {
// //                     TempData["GoogleLoginError"] = "Không thể lấy Google User ID từ ID Token (sau khi parse/validate).";
// //                     Console.WriteLine("GoogleLogin POST Error: Google User ID (sub) is missing from ID Token payload after parse/validate.");
// //                     return RedirectToAction("Login", new { returnUrl });
// //                 }

// //                 // ... (Phần code tìm hoặc tạo user còn lại giữ nguyên như trước) ...
// //                 // ... (Ví dụ: var user = await _context.Users.FirstOrDefaultAsync(u => u.GoogleId == googleUserId); ) ...
// //                 // ... (Phần này sẽ giống hệt code bạn đã có) ...
// //                 var user = await _context.Users.FirstOrDefaultAsync(u => u.GoogleId == googleUserId);

// //                 if (user != null)
// //                 {
// //                     Console.WriteLine($"GoogleLogin POST: User found with GoogleId '{googleUserId}'. Username: '{user.Username}'.");
// //                     await SignInUser(user, false);
// //                 }
// //                 else
// //                 {
// //                     Console.WriteLine($"GoogleLogin POST: No user found with GoogleId '{googleUserId}'. Attempting to create new user.");
// //                     if (!string.IsNullOrEmpty(email))
// //                     {
// //                         var existingUserWithEmail = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
// //                         if (existingUserWithEmail != null)
// //                         {
// //                             TempData["GoogleLoginError"] = $"Đã tồn tại tài khoản khác ({existingUserWithEmail.Username}) với email {email}. Vui lòng đăng nhập bằng tài khoản đó và liên kết Google từ trang cá nhân (nếu có chức năng đó), hoặc sử dụng một email khác.";
// //                             Console.WriteLine($"GoogleLogin POST Error: Email '{email}' already exists for another user '{existingUserWithEmail.Username}'.");
// //                             return RedirectToAction("Login", new { returnUrl });
// //                         }
// //                     }

// //                     var newUsername = !string.IsNullOrEmpty(email) ? email : $"gg_{googleUserId}";
// //                     if (await _context.Users.AnyAsync(u => u.Username == newUsername))
// //                     {
// //                         newUsername = $"gg_{googleUserId}_{Guid.NewGuid().ToString("N").Substring(0, 6)}";
// //                     }
// //                     Console.WriteLine($"GoogleLogin POST: Final username for new Google user: '{newUsername}'.");

// //                     var newUser = new User
// //                     {
// //                         Username = newUsername,
// //                         Email = email,
// //                         FullName = fullName ?? "Người dùng Google",
// //                         GoogleId = googleUserId,
// //                         Role = "User",
// //                         CreatedAt = DateTime.Now,
// //                         Password = GenerateRandomPasswordPlaceholder(),
// //                         AvatarUrl = payload.Picture ?? "/images/default-avatar.png"
// //                     };

// //                     _context.Users.Add(newUser);
// //                     await _context.SaveChangesAsync();
// //                     Console.WriteLine($"GoogleLogin POST: New user '{newUser.Username}' created successfully with UserId '{newUser.UserId}'.");
// //                     await SignInUser(newUser, false);
// //                 }

// //                 if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
// //                 {
// //                     Console.WriteLine($"GoogleLogin POST: Redirecting to local ReturnUrl: {returnUrl}");
// //                     return Redirect(returnUrl);
// //                 }
// //                 Console.WriteLine("GoogleLogin POST: Redirecting to Home/Index.");
// //                 return RedirectToAction("Index", "Home");

// //             }
// //             catch (DbUpdateException dbEx)
// //             {
// //                 Console.WriteLine($"GoogleLogin POST DbUpdateException: {dbEx.ToString()}");
// //                 string innerErrorMessages = GetInnerExceptionMessages(dbEx);
// //                 TempData["GoogleLoginError"] = "Lỗi khi lưu dữ liệu người dùng Google. Chi tiết: " + innerErrorMessages;
// //                 return RedirectToAction("Login", new { returnUrl });
// //             }
// //             catch (Exception ex)
// //             {
// //                 TempData["GoogleLoginError"] = "Lỗi không xác định khi đăng nhập bằng Google: " + ex.Message;
// //                 Console.WriteLine($"GoogleLogin POST General Exception: {ex.ToString()}");
// //                 return RedirectToAction("Login", new { returnUrl });
// //             }
// //         }
// //         // ============================================================
// //         // KẾT THÚC ACTION GOOGLE LOGIN
// //         // ============================================================


// //         // GET: Accounts/Logout
// //         public async Task<IActionResult> Logout()
// //         {
// //             Console.WriteLine("AccountsController: Logout action called.");
// //             await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
// //             Console.WriteLine("Logout: User signed out.");
// //             return RedirectToAction("Login", "Accounts");
// //         }


// //         private async Task SignInUser(User user, bool isPersistent)
// //         {
// //             Console.WriteLine($"SignInUser: Attempting to sign in user '{user.Username}'. IsPersistent: {isPersistent}. UserId: {user.UserId}");
// //             try
// //             {
// //                 user.LastLoginAt = DateTime.Now;
// //                 _context.Users.Update(user);
// //                 Console.WriteLine($"SignInUser: Attempting to save LastLoginAt for user '{user.Username}'.");
// //                 await _context.SaveChangesAsync();
// //                 Console.WriteLine($"SignInUser: Updated LastLoginAt for user '{user.Username}'.");
// //             }
// //             catch (DbUpdateException dbEx)
// //             {
// //                 Console.WriteLine($"SignInUser DbUpdateException: {dbEx.ToString()}");
// //                 string innerErrorMessages = GetInnerExceptionMessages(dbEx);
// //                 Console.WriteLine("SignInUser Inner Exception Details: " + innerErrorMessages);
// //             }
// //             catch (Exception ex)
// //             {
// //                 Console.WriteLine($"SignInUser General Exception while updating LastLoginAt: {ex.ToString()}");
// //             }

// //             var claims = new List<Claim>
// //             {
// //                 new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
// //                 new Claim(ClaimTypes.Name, user.Username),
// //                 new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
// //                 new Claim(ClaimTypes.Role, user.Role)
// //             };
// //             Console.WriteLine($"SignInUser: Created claims for user '{user.Username}'.");
// //             var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
// //             var authProperties = new AuthenticationProperties { IsPersistent = isPersistent };
// //             await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
// //             Console.WriteLine($"SignInUser: User '{user.Username}' successfully signed in with cookie.");
// //         }

// //         private string GetInnerExceptionMessages(Exception ex)
// //         {
// //             string messages = ex.Message;
// //             Exception? inner = ex.InnerException;
// //             int i = 0;
// //             while (inner != null)
// //             {
// //                 messages += $" | Inner ({i++}): {inner.Message}";
// //                 inner = inner.InnerException;
// //             }
// //             return messages;
// //         }

// //         private string GenerateRandomPasswordPlaceholder(int length = 32)
// //         {
// //             const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()-_=+[]{}|;:,.<>/?";
// //             using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
// //             {
// //                 var byteBuffer = new byte[length];
// //                 rng.GetBytes(byteBuffer);
// //                 var chars = new char[length];
// //                 for (int i = 0; i < length; i++)
// //                 {
// //                     chars[i] = validChars[byteBuffer[i] % validChars.Length];
// //                 }
// //                 return new string(chars);
// //             }
// //         }
// //     }
// // }
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using System.Security.Claims;
// using Microsoft.AspNetCore.Authentication;
// using Microsoft.AspNetCore.Authentication.Cookies;
// using TourismWeb.Models;
// using System.Threading.Tasks;
// using System.Linq;
// using System.Collections.Generic;
// using System;
// using Microsoft.Extensions.Configuration;
// using System.Net.Http;
// using Newtonsoft.Json.Linq;
// using System.ComponentModel.DataAnnotations;
// // using Microsoft.Extensions.Logging; // Bạn có thể bỏ comment nếu dùng ILogger
// using Google.Apis.Auth; // Cho Google Sign-In
// using System.Security.Cryptography; // Cho RandomNumberGenerator

// // ĐẢM BẢO USING NÀY ĐÚNG VỚI THƯ MỤC CHỨA VIEWMODEL CỦA BẠN
// using TourismWeb.Models.ViewModels; // Hoặc TourismWeb.Models.ViewModels, v.v.



// namespace TourismWeb.Controllers
// {
//     // Các ViewModel FacebookLoginViewModel và GoogleLoginViewModel có thể tạm thời ở đây
//     // NHƯNG ForgotPasswordViewModel và ResetPasswordViewModel PHẢI ở file riêng
//     // và được tham chiếu qua 'using TourismWeb.ViewModels;' ở trên.

//     public class FacebookLoginViewModel // Tạm thời để đây
//     {
//         [Required]
//         public string FacebookUserId { get; set; } = null!;
//         public string? Email { get; set; }
//         public string? FullName { get; set; }
//         [Required]
//         public string AccessToken { get; set; } = null!;
//     }

//     public class GoogleLoginViewModel // Tạm thời để đây
//     {
//         [Required]
//         public string IdToken { get; set; } = null!;
//     }

//     public class AccountsController : Controller
//     {
//         private readonly ApplicationDbContext _context;
//         private readonly IConfiguration _configuration;
//         private readonly IHttpClientFactory _httpClientFactory;
//         // private readonly IEmailSender _emailSender; // Sẽ cần khi triển khai gửi email thật
//         // private readonly ILogger<AccountsController> _logger;

//         public AccountsController(ApplicationDbContext context,
//                                   IConfiguration configuration,
//                                   IHttpClientFactory httpClientFactory
//                                   /*, IEmailSender emailSender, ILogger<AccountsController> logger */)
//         {
//             _context = context;
//             _configuration = configuration;
//             _httpClientFactory = httpClientFactory;
//             // _emailSender = emailSender;
//             // _logger = logger;
//         }

//         // GET: Accounts/Register
//         public IActionResult Register()
//         {
//             Console.WriteLine("AccountsController: Register GET action called.");
//             return View();
//         }

//         // POST: Accounts/Register
//         [HttpPost]
//         [ValidateAntiForgeryToken]
//         public async Task<IActionResult> Register(User model, string ConfirmPassword)
//         {
//             Console.WriteLine("AccountsController: Register POST action called.");
//             if (ModelState.IsValid)
//             {
//                 Console.WriteLine("Register POST: ModelState is Valid.");
//                 // Vì không hash, nên kiểm tra Password gốc
//                 if (model.Password != ConfirmPassword)
//                 {
//                     ViewBag.ErrorMessage = "Mật khẩu xác nhận không khớp!";
//                     Console.WriteLine("Register POST Error: Passwords do not match.");
//                     return View(model);
//                 }

//                 if (await _context.Users.AnyAsync(u => u.Username == model.Username))
//                 {
//                     ViewBag.ErrorMessage = "Tên đăng nhập đã tồn tại!";
//                     Console.WriteLine($"Register POST Error: Username '{model.Username}' already exists.");
//                     return View(model);
//                 }
//                 if (await _context.Users.AnyAsync(u => u.Email == model.Email))
//                 {
//                     ViewBag.ErrorMessage = "Email đã tồn tại!";
//                     Console.WriteLine($"Register POST Error: Email '{model.Email}' already exists.");
//                     return View(model);
//                 }

//                 // Mật khẩu được gán trực tiếp từ model (KHÔNG AN TOÀN)
//                 // model.Password đã có giá trị từ form

//                 model.CreatedAt = DateTime.Now;
//                 model.Role = "User";

//                 try
//                 {
//                     _context.Users.Add(model);
//                     await _context.SaveChangesAsync();
//                     Console.WriteLine($"Register POST: User '{model.Username}' created successfully.");

//                     TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
//                     return RedirectToAction("Login");
//                 }
//                 catch (DbUpdateException dbEx)
//                 {
//                     Console.WriteLine($"Register POST DbUpdateException: {dbEx.ToString()}");
//                     string innerErrorMessages = GetInnerExceptionMessages(dbEx);
//                     ViewBag.ErrorMessage = "Lỗi khi lưu dữ liệu vào database (Register). Chi tiết: " + innerErrorMessages;
//                     Console.WriteLine("Register POST Inner Exception Details: " + innerErrorMessages);
//                     return View(model);
//                 }
//                 catch (Exception ex)
//                 {
//                     ViewBag.ErrorMessage = "Lỗi lưu dữ liệu không xác định. Vui lòng thử lại.";
//                     Console.WriteLine($"Register POST General Exception: {ex.ToString()}");
//                     return View(model);
//                 }
//             }
//             else
//             {
//                 var errors = ModelState.Values.SelectMany(v => v.Errors)
//                                             .Select(e => e.ErrorMessage)
//                                             .ToList();
//                 ViewBag.ErrorMessage = string.Join("<br>", errors);
//                 Console.WriteLine($"Register POST Error: ModelState is Invalid. Errors: {ViewBag.ErrorMessage}");
//                 return View(model);
//             }
//         }


//         // GET: Accounts/Login
//         public IActionResult Login(string? returnUrl = null)
//         {
//             Console.WriteLine($"AccountsController: Login GET action called. ReturnUrl: {returnUrl}");
//             ViewData["ReturnUrl"] = returnUrl ?? string.Empty; // Sửa CS8601
//             if (TempData["SuccessMessage"] != null)
//             {
//                 ViewBag.SuccessMessage = TempData["SuccessMessage"];
//             }
//             if (TempData["ResetPasswordError"] != null) // Hiển thị lỗi từ ResetPassword GET
//             {
//                 ViewBag.ErrorMessage = TempData["ResetPasswordError"];
//             }
//             return View();
//         }

//         // POST: Accounts/Login
//         [HttpPost]
//         [ValidateAntiForgeryToken]
//         public async Task<IActionResult> Login(string UsernameOrEmail, string Password, bool RememberMe, string? returnUrl = null)
//         {
//             Console.WriteLine($"AccountsController: Login POST action called. UsernameOrEmail: {UsernameOrEmail}, RememberMe: {RememberMe}, ReturnUrl: {returnUrl}");
//             ViewData["ReturnUrl"] = returnUrl ?? string.Empty; // Sửa CS8601
//             if (string.IsNullOrEmpty(UsernameOrEmail) || string.IsNullOrEmpty(Password))
//             {
//                 ViewBag.ErrorMessage = "Vui lòng nhập đầy đủ thông tin!";
//                 Console.WriteLine("Login POST Error: Username/Email or Password is empty.");
//                 return View();
//             }

//             // So sánh mật khẩu gốc (KHÔNG AN TOÀN)
//             var user = await _context.Users
//                 .FirstOrDefaultAsync(u => (u.Username == UsernameOrEmail || u.Email == UsernameOrEmail) && u.Password == Password);

//             if (user == null)
//             {
//                 ViewBag.ErrorMessage = "Tên đăng nhập, email hoặc mật khẩu không chính xác!";
//                 Console.WriteLine("Login POST Error: Invalid username, email, or password.");
//                 return View();
//             }

//             // Kiểm tra nếu là tài khoản mạng xã hội mà lại cố đăng nhập bằng form thường
//             // Giả sử Password của tài khoản mạng xã hội là null hoặc là placeholder đã biết
//             if (string.IsNullOrEmpty(user.Password) && (user.FacebookId != null || user.GoogleId != null))
//             {
//                 ViewBag.ErrorMessage = "Tài khoản này được đăng ký qua mạng xã hội. Vui lòng đăng nhập qua Facebook hoặc Google.";
//                 Console.WriteLine($"Login POST Error: User '{user.Username}' tried to login with password but is a social account without a set password (or placeholder).");
//                 return View();
//             }


//             Console.WriteLine($"Login POST: User '{user.Username}' found and password (plain text) matched.");
//             await SignInUser(user, RememberMe);
//             Console.WriteLine($"Login POST: User '{user.Username}' signed in.");

//             if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
//             {
//                 Console.WriteLine($"Login POST: Redirecting to local ReturnUrl: {returnUrl}");
//                 return Redirect(returnUrl);
//             }
//             Console.WriteLine("Login POST: Redirecting to Home/Index.");
//             return RedirectToAction("Index", "Home");
//         }

//         // ============================================================
//         // QUÊN MẬT KHẨU & ĐẶT LẠI MẬT KHẨU
//         // ============================================================

//         // GET: /Accounts/ForgotPassword
//         public IActionResult ForgotPassword()
//         {
//             Console.WriteLine("AccountsController: ForgotPassword GET action called.");
//             return View();
//         }

//         [HttpPost]
//         [ValidateAntiForgeryToken]
//         public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model) // Sử dụng ViewModel đã tách file
//         {
//             Console.WriteLine($"AccountsController: ForgotPassword POST action called. Email: {model.Email}");
//             if (ModelState.IsValid)
//             {
//                 var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
//                 if (user != null)
//                 {
//                     // Không cho phép đặt lại mật khẩu nếu tài khoản được tạo qua Facebook/Google và chưa có mật khẩu
//                     // (giả sử mật khẩu placeholder của tài khoản xã hội là khác rỗng)
//                     if (user.Password == GenerateRandomPasswordPlaceholder() && (user.FacebookId != null || user.GoogleId != null))
//                     {
//                         Console.WriteLine($"ForgotPassword POST: Attempt to reset password for social account '{user.Email}' that uses social login placeholder password.");
//                         ViewBag.Message = "Tài khoản này được đăng ký qua mạng xã hội. Vui lòng sử dụng phương thức đăng nhập tương ứng.";
//                         return View("ForgotPasswordConfirmation");
//                     }
//                     if (string.IsNullOrEmpty(user.Password) && (user.FacebookId != null || user.GoogleId != null)) // Trường hợp cột Password là NULLABLE
//                     {
//                         Console.WriteLine($"ForgotPassword POST: Attempt to reset password for social account '{user.Email}' with NULL password.");
//                         ViewBag.Message = "Tài khoản này được đăng ký qua mạng xã hội. Vui lòng sử dụng phương thức đăng nhập tương ứng.";
//                         return View("ForgotPasswordConfirmation");
//                     }


//                     user.PasswordResetToken = GenerateSecurePasswordResetToken();
//                     user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
//                     Console.WriteLine($"ForgotPassword POST: Generated reset token for user '{user.Email}': {user.PasswordResetToken}, Expires: {user.PasswordResetTokenExpiry}");

//                     _context.Users.Update(user);
//                     await _context.SaveChangesAsync();
//                     Console.WriteLine($"ForgotPassword POST: Reset token saved for user '{user.Email}'.");

//                     var resetLink = Url.Action("ResetPassword", "Accounts",
//                                                new { token = user.PasswordResetToken }, Request.Scheme);

//                     // BẠN CẦN TRIỂN KHAI DỊCH VỤ GỬI EMAIL THỰC SỰ Ở ĐÂY
//                     // await _emailSender.SendEmailAsync(model.Email, "Yêu cầu đặt lại mật khẩu",
//                     //    $"Vui lòng đặt lại mật khẩu của bạn bằng cách nhấp vào link sau: <a href='{resetLink}'>Đặt lại mật khẩu</a>");
//                     Console.WriteLine($"DEBUG - Reset Link for {model.Email}: {resetLink}");

//                     ViewBag.Message = "Nếu email của bạn tồn tại trong hệ thống và hợp lệ để đặt lại mật khẩu, một hướng dẫn đã được gửi đến. Vui lòng kiểm tra hộp thư của bạn.";
//                 }
//                 else
//                 {
//                     Console.WriteLine($"ForgotPassword POST: Email '{model.Email}' not found in the system.");
//                     ViewBag.Message = "Nếu email của bạn tồn tại trong hệ thống và hợp lệ để đặt lại mật khẩu, một hướng dẫn đã được gửi đến. Vui lòng kiểm tra hộp thư của bạn.";
//                 }
//                 return View("ForgotPasswordConfirmation");
//             }
//             Console.WriteLine("ForgotPassword POST Error: ModelState is Invalid.");
//             return View(model);
//         }

//         // GET: /Accounts/ResetPassword?token=xxxx
//         public async Task<IActionResult> ResetPassword(string token)
//         {
//             Console.WriteLine($"AccountsController: ResetPassword GET action called. Token: {token}");
//             if (string.IsNullOrEmpty(token))
//             {
//                 Console.WriteLine("ResetPassword GET Error: Token is null or empty.");
//                 TempData["ResetPasswordError"] = "Token đặt lại mật khẩu không được để trống.";
//                 return RedirectToAction("Login");
//             }

//             var user = await _context.Users.FirstOrDefaultAsync(u => u.PasswordResetToken == token &&
//                                                                    u.PasswordResetTokenExpiry > DateTime.UtcNow);
//             if (user == null)
//             {
//                 Console.WriteLine("ResetPassword GET Error: Invalid or expired token.");
//                 TempData["ResetPasswordError"] = "Link đặt lại mật khẩu không hợp lệ hoặc đã hết hạn. Vui lòng thử lại quá trình quên mật khẩu.";
//                 return RedirectToAction("Login"); // Chuyển về Login để hiển thị TempData
//             }

//             var model = new ResetPasswordViewModel { Token = token }; // Sử dụng ViewModel đã tách file
//             Console.WriteLine($"ResetPassword GET: Token is valid for user '{user.Email}'. Displaying reset form.");
//             return View(model);
//         }

//         [HttpPost]
//         [ValidateAntiForgeryToken]
//         public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model) // Sử dụng ViewModel đã tách file
//         {
//             Console.WriteLine($"AccountsController: ResetPassword POST action called. Token: {model.Token}");
//             if (!ModelState.IsValid)
//             {
//                 Console.WriteLine("ResetPassword POST Error: ModelState is Invalid.");
//                 return View(model);
//             }

//             var user = await _context.Users.FirstOrDefaultAsync(u => u.PasswordResetToken == model.Token &&
//                                                                    u.PasswordResetTokenExpiry > DateTime.UtcNow);
//             if (user == null)
//             {
//                 Console.WriteLine("ResetPassword POST Error: Invalid or expired token on POST.");
//                 // Gán lỗi vào ViewBag để View "ResetPassword" có thể hiển thị lại với lỗi
//                 // Hoặc tốt hơn là dùng TempData và Redirect để tránh F5 submit lại form
//                 TempData["ResetPasswordErrorOnPost"] = "Link đặt lại mật khẩu không hợp lệ hoặc đã hết hạn. Vui lòng thử lại quá trình quên mật khẩu.";
//                 return RedirectToAction("ForgotPassword");
//             }

//             // Cập nhật mật khẩu gốc (KHÔNG AN TOÀN)
//             user.Password = model.Password;
//             user.PasswordResetToken = null;
//             user.PasswordResetTokenExpiry = null;
//             Console.WriteLine($"ResetPassword POST: New plain text password set and token invalidated for user '{user.Email}'.");

//             _context.Users.Update(user);
//             await _context.SaveChangesAsync();
//             Console.WriteLine($"ResetPassword POST: User '{user.Email}' password updated successfully.");

//             TempData["SuccessMessage"] = "Mật khẩu của bạn đã được đặt lại thành công. Vui lòng đăng nhập.";
//             return RedirectToAction("Login");
//         }


//         // ============================================================
//         // ĐĂNG NHẬP MẠNG XÃ HỘI (Facebook, Google)
//         // ============================================================
//         // POST: Accounts/FacebookLogin
//         [HttpPost]
//         [ValidateAntiForgeryToken]
//         public async Task<IActionResult> FacebookLogin(FacebookLoginViewModel model, string? returnUrl = null)
//         {
//             // ... (Code FacebookLogin giữ nguyên như phiên bản trước,
//             // đảm bảo khi tạo newUser, Password được gán GenerateRandomPasswordPlaceholder()
//             // nếu cột Password trong DB của bạn là NOT NULL) ...
//             Console.WriteLine("AccountsController: FacebookLogin POST action called.");
//             Console.WriteLine($"FacebookLogin POST - Received Model: UserId='{model.FacebookUserId}', Email='{model.Email}', FullName='{model.FullName}', AccessToken IS {(string.IsNullOrEmpty(model.AccessToken) ? "EMPTY" : "PRESENT")}");
//             ViewData["ReturnUrl"] = returnUrl ?? string.Empty;

//             if (!ModelState.IsValid)
//             {
//                 var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
//                 TempData["FacebookLoginError"] = "Dữ liệu đăng nhập Facebook không hợp lệ (ModelState): " + string.Join("; ", errors);
//                 Console.WriteLine($"FacebookLogin POST Error: ModelState is Invalid. Errors: {string.Join("; ", errors)}");
//                 return RedirectToAction("Login", new { returnUrl });
//             }
//             Console.WriteLine("FacebookLogin POST: ModelState is Valid.");

//             var facebookAppId = _configuration["Authentication:Facebook:AppId"];
//             var facebookAppSecret = _configuration["Authentication:Facebook:AppSecret"];
//             Console.WriteLine($"FacebookLogin POST - Config: AppId='{facebookAppId}', AppSecret IS {(string.IsNullOrEmpty(facebookAppSecret) ? "EMPTY" : "PRESENT")}");


//             if (string.IsNullOrEmpty(facebookAppId) || string.IsNullOrEmpty(facebookAppSecret))
//             {
//                 TempData["FacebookLoginError"] = "Cấu hình Facebook App ID/App Secret bị thiếu trên server.";
//                 Console.WriteLine("FacebookLogin POST Error: Facebook AppId or AppSecret is missing in configuration.");
//                 return RedirectToAction("Login", new { returnUrl });
//             }

//             var httpClient = _httpClientFactory.CreateClient();
//             var debugTokenUrl = $"https://graph.facebook.com/debug_token?input_token={model.AccessToken}&access_token={facebookAppId}|{facebookAppSecret}";
//             Console.WriteLine($"FacebookLogin POST - Debug Token URL: {debugTokenUrl}");

//             try
//             {
//                 Console.WriteLine("FacebookLogin POST: Attempting to call Facebook debug_token API.");
//                 var response = await httpClient.GetAsync(debugTokenUrl);
//                 Console.WriteLine($"FacebookLogin POST - Debug Token API Response Status: {response.StatusCode}");

//                 if (!response.IsSuccessStatusCode)
//                 {
//                     var errorContent = await response.Content.ReadAsStringAsync();
//                     TempData["FacebookLoginError"] = $"Không thể xác thực token với Facebook. Status: {response.StatusCode}. Phản hồi từ FB: {errorContent}";
//                     Console.WriteLine($"FacebookLogin POST Error: Failed to validate token with Facebook. Status: {response.StatusCode}. FB Response: {errorContent}");
//                     return RedirectToAction("Login", new { returnUrl });
//                 }

//                 var content = await response.Content.ReadAsStringAsync();
//                 Console.WriteLine($"FacebookLogin POST - Debug Token API Success Response Content: {content}");
//                 var tokenValidationResult = JObject.Parse(content);
//                 var tokenData = tokenValidationResult["data"];

//                 if (tokenData == null)
//                 {
//                     TempData["FacebookLoginError"] = "Token Facebook không hợp lệ: không có trường 'data' trong phản hồi.";
//                     Console.WriteLine($"FacebookLogin POST Error: Invalid Facebook token - 'data' field missing in response. Full response: {content}");
//                     return RedirectToAction("Login", new { returnUrl });
//                 }

//                 bool isValid = tokenData["is_valid"]?.Value<bool>() ?? false;
//                 string? appIdFromToken = tokenData["app_id"]?.Value<string>();
//                 string? userIdFromToken = tokenData["user_id"]?.Value<string>();

//                 Console.WriteLine($"FacebookLogin POST - Token Validation: is_valid='{isValid}', app_id_from_token='{appIdFromToken}', user_id_from_token='{userIdFromToken}'");

//                 if (!isValid || appIdFromToken != facebookAppId || userIdFromToken != model.FacebookUserId)
//                 {
//                     TempData["FacebookLoginError"] = "Token Facebook không hợp lệ hoặc không khớp với ứng dụng/người dùng.";
//                     if (!isValid) Console.WriteLine("FacebookLogin POST Error: Token is_valid is false.");
//                     if (appIdFromToken != facebookAppId) Console.WriteLine($"FacebookLogin POST Error: AppId mismatch. Token AppId: {appIdFromToken}, Config AppId: {facebookAppId}.");
//                     if (userIdFromToken != model.FacebookUserId) Console.WriteLine($"FacebookLogin POST Error: UserId mismatch. Token UserId: {userIdFromToken}, Model UserId: {model.FacebookUserId}.");
//                     return RedirectToAction("Login", new { returnUrl });
//                 }
//                 Console.WriteLine("FacebookLogin POST: Token validation successful.");
//             }
//             catch (Exception ex)
//             {
//                 TempData["FacebookLoginError"] = "Lỗi khi xác thực token Facebook với server: " + ex.Message;
//                 Console.WriteLine($"FacebookLogin POST Exception during token validation: {ex.ToString()}");
//                 return RedirectToAction("Login", new { returnUrl });
//             }

//             try
//             {
//                 Console.WriteLine("FacebookLogin POST: Attempting to find or create user.");
//                 var user = await _context.Users.FirstOrDefaultAsync(u => u.FacebookId == model.FacebookUserId);

//                 if (user != null)
//                 {
//                     Console.WriteLine($"FacebookLogin POST: User found with FacebookId '{model.FacebookUserId}'. Username: '{user.Username}'.");
//                     await SignInUser(user, false);
//                     Console.WriteLine($"FacebookLogin POST: Existing user '{user.Username}' signed in.");
//                     if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
//                     {
//                         Console.WriteLine($"FacebookLogin POST: Redirecting existing user to local ReturnUrl: {returnUrl}");
//                         return Redirect(returnUrl);
//                     }
//                     Console.WriteLine("FacebookLogin POST: Redirecting existing user to Home/Index.");
//                     return RedirectToAction("Index", "Home");
//                 }
//                 else
//                 {
//                     Console.WriteLine($"FacebookLogin POST: No user found with FacebookId '{model.FacebookUserId}'. Attempting to create new user.");
//                     if (!string.IsNullOrEmpty(model.Email))
//                     {
//                         Console.WriteLine($"FacebookLogin POST: Checking if email '{model.Email}' already exists for another user.");
//                         var existingUserWithEmail = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
//                         if (existingUserWithEmail != null)
//                         {
//                             TempData["FacebookLoginError"] = $"Đã tồn tại tài khoản với email {model.Email}. Vui lòng đăng nhập bằng tài khoản đó và liên kết Facebook từ trang cá nhân (nếu muốn).";
//                             Console.WriteLine($"FacebookLogin POST Error: Email '{model.Email}' already exists for user '{existingUserWithEmail.Username}'.");
//                             return RedirectToAction("Login", new { returnUrl });
//                         }
//                         Console.WriteLine($"FacebookLogin POST: Email '{model.Email}' is available or not provided by Facebook.");
//                     }

//                     var newUsername = !string.IsNullOrEmpty(model.Email) ? model.Email : $"fb_{model.FacebookUserId}";
//                     if (await _context.Users.AnyAsync(u => u.Username == newUsername))
//                     {
//                         Console.WriteLine($"FacebookLogin POST: Generated username '{newUsername}' already exists. Generating a unique one.");
//                         newUsername = $"fb_{model.FacebookUserId}_{Guid.NewGuid().ToString("N").Substring(0, 6)}";
//                     }
//                     Console.WriteLine($"FacebookLogin POST: Final username for new user: '{newUsername}'.");

//                     var newUser = new User
//                     {
//                         Username = newUsername,
//                         Email = model.Email,
//                         FullName = model.FullName ?? "Người dùng Facebook",
//                         FacebookId = model.FacebookUserId,
//                         Role = "User",
//                         CreatedAt = DateTime.Now,
//                         Password = GenerateRandomPasswordPlaceholder() // Giữ lại để cột Password không null nếu DB yêu cầu
//                     };

//                     _context.Users.Add(newUser);
//                     Console.WriteLine($"FacebookLogin POST: Attempting to save new user '{newUser.Username}'. Details: Email='{newUser.Email}', FullName='{newUser.FullName}', FbId='{newUser.FacebookId}', PasswordPlaceholderSet=True");
//                     await _context.SaveChangesAsync();
//                     Console.WriteLine($"FacebookLogin POST: New user '{newUser.Username}' created successfully with UserId '{newUser.UserId}'.");

//                     await SignInUser(newUser, false);
//                     Console.WriteLine($"FacebookLogin POST: New user '{newUser.Username}' signed in.");
//                     if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
//                     {
//                         Console.WriteLine($"FacebookLogin POST: Redirecting new user to local ReturnUrl: {returnUrl}");
//                         return Redirect(returnUrl);
//                     }
//                     Console.WriteLine("FacebookLogin POST: Redirecting new user to Home/Index.");
//                     return RedirectToAction("Index", "Home");
//                 }
//             }
//             catch (DbUpdateException dbEx)
//             {
//                 Console.WriteLine($"FacebookLogin POST DbUpdateException: {dbEx.ToString()}");
//                 string innerErrorMessages = GetInnerExceptionMessages(dbEx);
//                 TempData["FacebookLoginError"] = "Lỗi khi lưu dữ liệu người dùng Facebook. Chi tiết: " + innerErrorMessages;
//                 Console.WriteLine("FacebookLogin POST Inner Exception Details: " + innerErrorMessages);
//                 return RedirectToAction("Login", new { returnUrl });
//             }
//             catch (Exception ex)
//             {
//                 TempData["FacebookLoginError"] = "Lỗi không xác định khi đăng nhập Facebook: " + ex.Message;
//                 Console.WriteLine($"FacebookLogin POST General Exception: {ex.ToString()}");
//                 return RedirectToAction("Login", new { returnUrl });
//             }
//         }


//         // POST: Accounts/GoogleLogin
//         [HttpPost]
//         [ValidateAntiForgeryToken]
//         public async Task<IActionResult> GoogleLogin(GoogleLoginViewModel model, string? returnUrl = null)
//         {
//             // ... (Code GoogleLogin giữ nguyên như phiên bản trước,
//             // đảm bảo khi tạo newUser, Password được gán GenerateRandomPasswordPlaceholder()
//             // nếu cột Password trong DB của bạn là NOT NULL) ...
//             Console.WriteLine("AccountsController: GoogleLogin POST action called.");
//             Console.WriteLine($"GoogleLogin POST - Received IdToken: {(string.IsNullOrEmpty(model.IdToken) ? "EMPTY" : "PRESENT (length: " + model.IdToken.Length + ")")}");
//             ViewData["ReturnUrl"] = returnUrl ?? string.Empty;

//             bool isOfflineDebugMode = false; // Đặt thành true nếu đang test offline và không có internet

//             if (!ModelState.IsValid || string.IsNullOrEmpty(model.IdToken))
//             {
//                 TempData["GoogleLoginError"] = "Dữ liệu đăng nhập Google không hợp lệ (ID Token bị thiếu).";
//                 Console.WriteLine("GoogleLogin POST Error: ModelState is Invalid or IdToken is missing.");
//                 return RedirectToAction("Login", new { returnUrl });
//             }

//             try
//             {
//                 var googleClientId = _configuration["Authentication:Google:ClientId"];
//                 if (string.IsNullOrEmpty(googleClientId))
//                 {
//                     TempData["GoogleLoginError"] = "Cấu hình Google Client ID bị thiếu trên server.";
//                     Console.WriteLine("GoogleLogin POST Error: Google ClientID is missing in configuration.");
//                     return RedirectToAction("Login", new { returnUrl });
//                 }
//                 Console.WriteLine($"GoogleLogin POST - Using Google Client ID from config: {googleClientId}");

//                 GoogleJsonWebSignature.Payload payload;

//                 if (isOfflineDebugMode)
//                 {
//                     Console.WriteLine("GoogleLogin POST WARNING: OFFLINE DEBUG MODE - SKIPPING ID TOKEN SIGNATURE VALIDATION. THIS IS NOT SAFE FOR PRODUCTION!");
//                     try
//                     {
//                         var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
//                         var jsonToken = handler.ReadJwtToken(model.IdToken);
//                         payload = new GoogleJsonWebSignature.Payload
//                         {
//                             Subject = jsonToken.Claims.FirstOrDefault(claim => claim.Type == "sub")?.Value,
//                             Email = jsonToken.Claims.FirstOrDefault(claim => claim.Type == "email")?.Value,
//                             Name = jsonToken.Claims.FirstOrDefault(claim => claim.Type == "name")?.Value,
//                             Picture = jsonToken.Claims.FirstOrDefault(claim => claim.Type == "picture")?.Value,
//                             EmailVerified = bool.TryParse(jsonToken.Claims.FirstOrDefault(claim => claim.Type == "email_verified")?.Value, out var ev) ? ev : false,
//                             Audience = jsonToken.Audiences?.ToList()
//                         };
//                         Console.WriteLine($"GoogleLogin POST (Offline Mode): Parsed Payload. Google User ID (sub): {payload.Subject}, Email: {payload.Email}, Name: {payload.Name}");
//                         bool audienceMatch = false;
//                         if (payload.Audience is string audString && audString == googleClientId) audienceMatch = true;
//                         else if (payload.Audience is IList<string> audList && audList.Contains(googleClientId)) audienceMatch = true;
//                         if (!audienceMatch) throw new InvalidJwtException($"Audience của ID Token không khớp với Client ID ('{googleClientId}') trong chế độ offline. Audience từ token: {string.Join(", ", jsonToken.Audiences ?? new List<string>())}");
//                         Console.WriteLine("GoogleLogin POST (Offline Mode): Audience check passed.");
//                     }
//                     catch (Exception parseEx)
//                     {
//                         TempData["GoogleLoginError"] = "Lỗi parse ID Token (chế độ offline debug): " + parseEx.Message;
//                         Console.WriteLine($"GoogleLogin POST Error: Failed to parse ID Token in offline debug mode. Exception: {parseEx.ToString()}");
//                         return RedirectToAction("Login", new { returnUrl });
//                     }
//                 }
//                 else
//                 {
//                     try
//                     {
//                         var validationSettings = new GoogleJsonWebSignature.ValidationSettings
//                         {
//                             Audience = new[] { googleClientId }
//                         };
//                         Console.WriteLine("GoogleLogin POST: Attempting to validate Google ID Token online...");
//                         payload = await GoogleJsonWebSignature.ValidateAsync(model.IdToken, validationSettings);
//                         Console.WriteLine($"GoogleLogin POST: ID Token validated online. Google User ID (sub): {payload.Subject}, Email: {payload.Email}, Name: {payload.Name}, EmailVerified: {payload.EmailVerified}");
//                     }
//                     catch (Exception ex)
//                     {
//                         string errorMessage = "ID Token của Google không hợp lệ hoặc không thể xác thực: " + ex.Message;
//                         Console.WriteLine($"GoogleLogin POST Error: Invalid Google ID Token (Online Mode). Main Exception: {ex.ToString()}");
//                         if (ex.InnerException != null)
//                         {
//                             errorMessage += " | Inner Exception: " + ex.InnerException.Message;
//                             Console.WriteLine($"GoogleLogin POST Error - INNER EXCEPTION (Online Mode): {ex.InnerException.ToString()}");
//                         }
//                         TempData["GoogleLoginError"] = errorMessage;
//                         return RedirectToAction("Login", new { returnUrl });
//                     }
//                 }

//                 var googleUserId = payload.Subject;
//                 var email = payload.Email;
//                 var fullName = payload.Name;

//                 if (string.IsNullOrEmpty(googleUserId))
//                 {
//                     TempData["GoogleLoginError"] = "Không thể lấy Google User ID từ ID Token (sau khi parse/validate).";
//                     Console.WriteLine("GoogleLogin POST Error: Google User ID (sub) is missing from ID Token payload after parse/validate.");
//                     return RedirectToAction("Login", new { returnUrl });
//                 }
//                 var user = await _context.Users.FirstOrDefaultAsync(u => u.GoogleId == googleUserId);

//                 if (user != null)
//                 {
//                     Console.WriteLine($"GoogleLogin POST: User found with GoogleId '{googleUserId}'. Username: '{user.Username}'.");
//                     await SignInUser(user, false);
//                 }
//                 else
//                 {
//                     Console.WriteLine($"GoogleLogin POST: No user found with GoogleId '{googleUserId}'. Attempting to create new user.");
//                     if (!string.IsNullOrEmpty(email))
//                     {
//                         var existingUserWithEmail = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
//                         if (existingUserWithEmail != null)
//                         {
//                             TempData["GoogleLoginError"] = $"Đã tồn tại tài khoản khác ({existingUserWithEmail.Username}) với email {email}. Vui lòng đăng nhập bằng tài khoản đó và liên kết Google từ trang cá nhân (nếu có chức năng đó), hoặc sử dụng một email khác.";
//                             Console.WriteLine($"GoogleLogin POST Error: Email '{email}' already exists for another user '{existingUserWithEmail.Username}'.");
//                             return RedirectToAction("Login", new { returnUrl });
//                         }
//                     }

//                     var newUsername = !string.IsNullOrEmpty(email) ? email : $"gg_{googleUserId}";
//                     if (await _context.Users.AnyAsync(u => u.Username == newUsername))
//                     {
//                         newUsername = $"gg_{googleUserId}_{Guid.NewGuid().ToString("N").Substring(0, 6)}";
//                     }
//                     Console.WriteLine($"GoogleLogin POST: Final username for new Google user: '{newUsername}'.");

//                     var newUser = new User
//                     {
//                         Username = newUsername,
//                         Email = email,
//                         FullName = fullName ?? "Người dùng Google",
//                         GoogleId = googleUserId,
//                         Role = "User",
//                         CreatedAt = DateTime.Now,
//                         Password = GenerateRandomPasswordPlaceholder(), // Giữ lại để cột Password không null nếu DB yêu cầu
//                         AvatarUrl = payload.Picture ?? "/images/default-avatar.png"
//                     };

//                     _context.Users.Add(newUser);
//                     await _context.SaveChangesAsync();
//                     Console.WriteLine($"GoogleLogin POST: New user '{newUser.Username}' created successfully with UserId '{newUser.UserId}'.");
//                     await SignInUser(newUser, false);
//                 }

//                 if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
//                 {
//                     Console.WriteLine($"GoogleLogin POST: Redirecting to local ReturnUrl: {returnUrl}");
//                     return Redirect(returnUrl);
//                 }
//                 Console.WriteLine("GoogleLogin POST: Redirecting to Home/Index.");
//                 return RedirectToAction("Index", "Home");

//             }
//             catch (DbUpdateException dbEx)
//             {
//                 Console.WriteLine($"GoogleLogin POST DbUpdateException: {dbEx.ToString()}");
//                 string innerErrorMessages = GetInnerExceptionMessages(dbEx);
//                 TempData["GoogleLoginError"] = "Lỗi khi lưu dữ liệu người dùng Google. Chi tiết: " + innerErrorMessages;
//                 return RedirectToAction("Login", new { returnUrl });
//             }
//             catch (Exception ex)
//             {
//                 TempData["GoogleLoginError"] = "Lỗi không xác định khi đăng nhập bằng Google: " + ex.Message;
//                 Console.WriteLine($"GoogleLogin POST General Exception: {ex.ToString()}");
//                 return RedirectToAction("Login", new { returnUrl });
//             }
//         }


//         // GET: Accounts/Logout
//         public async Task<IActionResult> Logout()
//         {
//             Console.WriteLine("AccountsController: Logout action called.");
//             await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
//             Console.WriteLine("Logout: User signed out.");
//             return RedirectToAction("Login", "Accounts");
//         }


//         private async Task SignInUser(User user, bool isPersistent)
//         {
//             Console.WriteLine($"SignInUser: Attempting to sign in user '{user.Username}'. IsPersistent: {isPersistent}. UserId: {user.UserId}");
//             try
//             {
//                 user.LastLoginAt = DateTime.Now;
//                 _context.Users.Update(user);
//                 Console.WriteLine($"SignInUser: Attempting to save LastLoginAt for user '{user.Username}'.");
//                 await _context.SaveChangesAsync();
//                 Console.WriteLine($"SignInUser: Updated LastLoginAt for user '{user.Username}'.");
//             }
//             catch (DbUpdateException dbEx)
//             {
//                 Console.WriteLine($"SignInUser DbUpdateException while saving LastLoginAt: {dbEx.ToString()}");
//                 string innerErrorMessages = GetInnerExceptionMessages(dbEx);
//                 Console.WriteLine("SignInUser Inner Exception Details for LastLoginAt: " + innerErrorMessages);
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine($"SignInUser General Exception while updating LastLoginAt: {ex.ToString()}");
//             }

//             var claims = new List<Claim>
//             {
//                 new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
//                 new Claim(ClaimTypes.Name, user.Username),
//                 new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
//                 new Claim(ClaimTypes.Role, user.Role)
//             };
//             Console.WriteLine($"SignInUser: Created claims for user '{user.Username}'.");
//             var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
//             var authProperties = new AuthenticationProperties { IsPersistent = isPersistent };
//             await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
//             Console.WriteLine($"SignInUser: User '{user.Username}' successfully signed in with cookie.");
//         }

//         private string GetInnerExceptionMessages(Exception ex)
//         {
//             string messages = ex.Message;
//             Exception? inner = ex.InnerException;
//             int i = 0;
//             while (inner != null)
//             {
//                 messages += $" | Inner ({i++}): {inner.Message}";
//                 inner = inner.InnerException;
//             }
//             return messages;
//         }

//         private string GenerateRandomPasswordPlaceholder(int length = 32)
//         {
//             const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()-_=+[]{}|;:,.<>/?";
//             using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
//             {
//                 var byteBuffer = new byte[length];
//                 rng.GetBytes(byteBuffer);
//                 var chars = new char[length];
//                 for (int i = 0; i < length; i++)
//                 {
//                     chars[i] = validChars[byteBuffer[i] % validChars.Length];
//                 }
//                 return new string(chars);
//             }
//         }

//         private string GenerateSecurePasswordResetToken()
//         {
//             byte[] tokenBytes = new byte[32];
//             using (var rng = RandomNumberGenerator.Create())
//             {
//                 rng.GetBytes(tokenBytes);
//             }
//             return Convert.ToBase64String(tokenBytes)
//                 .TrimEnd('=')
//                 .Replace('+', '-')
//                 .Replace('/', '_');
//         }
//     }
// }
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using TourismWeb.Models; // Cho User model
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System;
using Microsoft.Extensions.Configuration; // Để đọc AppId, AppSecret
using System.Net.Http; // Cho HttpClient
using Newtonsoft.Json.Linq; // Cho việc parse JSON response từ Facebook
using System.ComponentModel.DataAnnotations;
using TourismWeb.Services; // Namespace của IEmailSender (BẠN CẦN TẠO VÀ ĐĂNG KÝ SERVICE NÀY)
// using Microsoft.Extensions.Logging; // Bỏ comment nếu bạn dùng ILogger
using Google.Apis.Auth; // Cho Google Sign-In
using System.Security.Cryptography; // Cho RandomNumberGenerator

// ĐẢM BẢO USING NÀY ĐÚNG VỚI THƯ MỤC VÀ NAMESPACE CỦA CÁC VIEWMODEL CỦA BẠN
using TourismWeb.Models.ViewModels; // Ví dụ: Nếu ViewModel của bạn nằm trong Models/ViewModels

namespace TourismWeb.Controllers
{
    // NẾU BẠN ĐÃ TÁCH CÁC VIEWMODEL FacebookLoginViewModel và GoogleLoginViewModel RA FILE RIÊNG
    // TRONG THƯ MỤC Models/ViewModels (VÀ ĐÃ USING Ở TRÊN), THÌ HÃY XÓA ĐỊNH NGHĨA CỦA CHÚNG Ở ĐÂY.
    // Nếu chưa tách, bạn có thể tạm thời để chúng lại ở đây.
    // public class FacebookLoginViewModel // Ví dụ nếu đã tách
    // {
    //     [Required]
    //     public string FacebookUserId { get; set; } = null!;
    //     public string? Email { get; set; }
    //     public string? FullName { get; set; }
    //     [Required]
    //     public string AccessToken { get; set; } = null!;
    // }

    // public class GoogleLoginViewModel // Ví dụ nếu đã tách
    // {
    //     [Required]
    //     public string IdToken { get; set; } = null!;
    // }

    public class AccountsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IEmailSender _emailSender; // <<--- KHAI BÁO IEmailSender
        // private readonly ILogger<AccountsController> _logger;

        public AccountsController(ApplicationDbContext context,
                                  IConfiguration configuration,
                                  IHttpClientFactory httpClientFactory,
                                  IEmailSender emailSender /*, ILogger<AccountsController> logger */) // <<--- INJECT IEmailSender
        {
            _context = context;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _emailSender = emailSender; // <<--- GÁN IEmailSender
            // _logger = logger;
        }

        // GET: Accounts/Register
        public IActionResult Register()
        {
            Console.WriteLine("AccountsController: Register GET action called.");
            return View();
        }

        // POST: Accounts/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(User model, string ConfirmPassword)
        {
            Console.WriteLine("AccountsController: Register POST action called.");
            if (ModelState.IsValid)
            {
                Console.WriteLine("Register POST: ModelState is Valid.");
                if (model.Password != ConfirmPassword)
                {
                    ViewBag.ErrorMessage = "Mật khẩu xác nhận không khớp!";
                    Console.WriteLine("Register POST Error: Passwords do not match.");
                    return View(model);
                }

                if (await _context.Users.AnyAsync(u => u.Username == model.Username))
                {
                    ViewBag.ErrorMessage = "Tên đăng nhập đã tồn tại!";
                    Console.WriteLine($"Register POST Error: Username '{model.Username}' already exists.");
                    return View(model);
                }
                if (await _context.Users.AnyAsync(u => u.Email == model.Email))
                {
                    ViewBag.ErrorMessage = "Email đã tồn tại!";
                    Console.WriteLine($"Register POST Error: Email '{model.Email}' already exists.");
                    return View(model);
                }

                model.CreatedAt = DateTime.Now;
                model.Role = "User";
                // Mật khẩu gốc được lưu (KHÔNG AN TOÀN)

                try
                {
                    _context.Users.Add(model);
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"Register POST: User '{model.Username}' created successfully.");

                    TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                    return RedirectToAction("Login");
                }
                catch (DbUpdateException dbEx)
                {
                    Console.WriteLine($"Register POST DbUpdateException: {dbEx.ToString()}");
                    string innerErrorMessages = GetInnerExceptionMessages(dbEx);
                    ViewBag.ErrorMessage = "Lỗi khi lưu dữ liệu vào database (Register). Chi tiết: " + innerErrorMessages;
                    Console.WriteLine("Register POST Inner Exception Details: " + innerErrorMessages);
                    return View(model);
                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMessage = "Lỗi lưu dữ liệu không xác định. Vui lòng thử lại.";
                    Console.WriteLine($"Register POST General Exception: {ex.ToString()}");
                    return View(model);
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                            .Select(e => e.ErrorMessage)
                                            .ToList();
                ViewBag.ErrorMessage = string.Join("<br>", errors);
                Console.WriteLine($"Register POST Error: ModelState is Invalid. Errors: {ViewBag.ErrorMessage}");
                return View(model);
            }
        }


        // GET: Accounts/Login
        public IActionResult Login(string? returnUrl = null)
        {
            Console.WriteLine($"AccountsController: Login GET action called. ReturnUrl: {returnUrl}");
            ViewData["ReturnUrl"] = returnUrl ?? string.Empty;
            if (TempData["SuccessMessage"] != null)
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"];
            }
            if (TempData["ResetPasswordError"] != null)
            {
                ViewBag.ErrorMessage = TempData["ResetPasswordError"];
            }
            if (TempData["ResetPasswordErrorOnPost"] != null) // Hiển thị lỗi từ ResetPassword POST
            {
                ViewBag.ErrorMessage = TempData["ResetPasswordErrorOnPost"];
            }
            return View();
        }

        // POST: Accounts/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string UsernameOrEmail, string Password, bool RememberMe, string? returnUrl = null)
        {
            Console.WriteLine($"AccountsController: Login POST action called. UsernameOrEmail: {UsernameOrEmail}, RememberMe: {RememberMe}, ReturnUrl: {returnUrl}");
            ViewData["ReturnUrl"] = returnUrl ?? string.Empty;
            if (string.IsNullOrEmpty(UsernameOrEmail) || string.IsNullOrEmpty(Password))
            {
                ViewBag.ErrorMessage = "Vui lòng nhập đầy đủ thông tin!";
                Console.WriteLine("Login POST Error: Username/Email or Password is empty.");
                return View();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => (u.Username == UsernameOrEmail || u.Email == UsernameOrEmail) && u.Password == Password);

            if (user == null)
            {
                ViewBag.ErrorMessage = "Tên đăng nhập, email hoặc mật khẩu không chính xác!";
                Console.WriteLine("Login POST Error: Invalid username, email, or password.");
                return View();
            }

            if (string.IsNullOrEmpty(user.Password) && (user.FacebookId != null || user.GoogleId != null))
            {
                ViewBag.ErrorMessage = "Tài khoản này được đăng ký qua mạng xã hội. Vui lòng đăng nhập qua Facebook hoặc Google.";
                Console.WriteLine($"Login POST Error: User '{user.Username}' tried to login with password but is a social account.");
                return View();
            }

            Console.WriteLine($"Login POST: User '{user.Username}' found.");
            await SignInUser(user, RememberMe);
            Console.WriteLine($"Login POST: User '{user.Username}' signed in.");

            return RedirectToLocal(ViewData["ReturnUrl"]?.ToString());
        }

        // ============================================================
        // QUÊN MẬT KHẨU & ĐẶT LẠI MẬT KHẨU
        // ============================================================

        // GET: /Accounts/ForgotPassword
        public IActionResult ForgotPassword()
        {
            Console.WriteLine("AccountsController: ForgotPassword GET action called.");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model) // Sử dụng ViewModel từ namespace đã using
        {
            Console.WriteLine($"AccountsController: ForgotPassword POST action called. Email: {model.Email}");
            if (ModelState.IsValid)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (user != null)
                {
                    if (string.IsNullOrEmpty(user.Password) && (user.FacebookId != null || user.GoogleId != null))
                    {
                        Console.WriteLine($"ForgotPassword POST: Attempt to reset password for social account '{user.Email}'.");
                        ViewBag.Message = "Tài khoản này được đăng ký qua mạng xã hội và không sử dụng mật khẩu truyền thống. Vui lòng sử dụng phương thức đăng nhập qua mạng xã hội tương ứng.";
                        return View("ForgotPasswordConfirmation");
                    }

                    user.PasswordResetToken = GenerateSecurePasswordResetToken();
                    user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
                    _context.Users.Update(user);
                    // Chưa SaveChanges() ở đây, đợi gửi email thành công

                    var resetLink = Url.Action("ResetPassword", "Accounts",
                                               new { token = user.PasswordResetToken }, Request.Scheme);

                    var emailSubject = "Yêu cầu đặt lại mật khẩu cho TourismWeb";
                    var emailMessage = $@"
                        <p>Chào {user.FullName ?? user.Username},</p>
                        <p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn tại TourismWeb.</p>
                        <p>Vui lòng nhấp vào liên kết sau để đặt lại mật khẩu của bạn:</p>
                        <p><a href='{resetLink}'>Đặt lại mật khẩu của tôi</a></p>
                        <p>Nếu bạn không yêu cầu điều này, vui lòng bỏ qua email này.</p>
                        <p>Liên kết này sẽ hết hạn sau 1 giờ.</p>
                        <p>Trân trọng,<br/>Đội ngũ TourismWeb</p>";

                    try
                    {
                        await _emailSender.SendEmailAsync(model.Email, emailSubject, emailMessage);
                        await _context.SaveChangesAsync(); // Lưu token nếu gửi email thành công
                        Console.WriteLine($"Password reset email sent to {model.Email}. Token saved. Link: {resetLink}");
                        ViewBag.Message = "Nếu email của bạn tồn tại trong hệ thống và hợp lệ, một hướng dẫn đặt lại mật khẩu đã được gửi đến. Vui lòng kiểm tra hộp thư của bạn (bao gồm cả thư mục Spam/Junk).";
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to send password reset email to {model.Email}. Exception: {ex.ToString()}");
                        // Không lưu token nếu gửi mail lỗi.
                        ViewBag.Message = "Đã có lỗi xảy ra trong quá trình gửi yêu cầu đặt lại mật khẩu. Vui lòng thử lại sau hoặc liên hệ hỗ trợ.";
                        // Không nên hiển thị lỗi chi tiết cho người dùng
                    }
                }
                else
                {
                    Console.WriteLine($"ForgotPassword POST: Email '{model.Email}' not found in the system.");
                    ViewBag.Message = "Nếu email của bạn tồn tại trong hệ thống và hợp lệ, một hướng dẫn đặt lại mật khẩu đã được gửi đến. Vui lòng kiểm tra hộp thư của bạn.";
                }
                return View("ForgotPasswordConfirmation");
            }
            Console.WriteLine("ForgotPassword POST Error: ModelState is Invalid.");
            return View(model);
        }

        // GET: /Accounts/ResetPassword?token=xxxx
        public async Task<IActionResult> ResetPassword(string token)
        {
            Console.WriteLine($"AccountsController: ResetPassword GET action called. Token: {token}");
            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("ResetPassword GET Error: Token is null or empty.");
                TempData["ResetPasswordError"] = "Token đặt lại mật khẩu không được để trống.";
                return RedirectToAction("Login");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.PasswordResetToken == token &&
                                                                   u.PasswordResetTokenExpiry > DateTime.UtcNow);
            if (user == null)
            {
                Console.WriteLine("ResetPassword GET Error: Invalid or expired token.");
                TempData["ResetPasswordError"] = "Link đặt lại mật khẩu không hợp lệ hoặc đã hết hạn. Vui lòng thử lại quá trình quên mật khẩu.";
                return RedirectToAction("Login");
            }

            var model = new ResetPasswordViewModel { Token = token };
            Console.WriteLine($"ResetPassword GET: Token is valid for user '{user.Email}'. Displaying reset form.");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            Console.WriteLine($"AccountsController: ResetPassword POST action called. Token: {model.Token}");
            if (!ModelState.IsValid)
            {
                Console.WriteLine("ResetPassword POST Error: ModelState is Invalid.");
                return View(model); // Giữ lại token trong model để view có thể render lại
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.PasswordResetToken == model.Token &&
                                                                   u.PasswordResetTokenExpiry > DateTime.UtcNow);
            if (user == null)
            {
                Console.WriteLine("ResetPassword POST Error: Invalid or expired token on POST.");
                // Quan trọng: Không hiển thị lại form với token cũ nếu nó không hợp lệ nữa
                // Thay vào đó, thông báo lỗi và yêu cầu người dùng bắt đầu lại
                TempData["ResetPasswordErrorOnPost"] = "Link đặt lại mật khẩu không hợp lệ hoặc đã hết hạn. Vui lòng yêu cầu một link mới.";
                return RedirectToAction("ForgotPassword");
            }

            user.Password = model.Password; // Cập nhật mật khẩu gốc (KHÔNG AN TOÀN)
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;
            Console.WriteLine($"ResetPassword POST: New plain text password set and token invalidated for user '{user.Email}'.");

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            Console.WriteLine($"ResetPassword POST: User '{user.Email}' password updated successfully.");

            TempData["SuccessMessage"] = "Mật khẩu của bạn đã được đặt lại thành công. Vui lòng đăng nhập.";
            return RedirectToAction("Login");
        }


        // ... (Các action FacebookLogin, GoogleLogin, Logout, và các hàm helper giữ nguyên như phiên bản trước) ...
        // Đảm bảo trong FacebookLogin và GoogleLogin, khi tạo newUser:
        // Password = GenerateRandomPasswordPlaceholder() // Được gọi nếu cột Password trong DB là NOT NULL
        // ... (Phần còn lại của các action FacebookLogin, GoogleLogin, Logout, SignInUser, GetInnerExceptionMessages, GenerateRandomPasswordPlaceholder, GenerateSecurePasswordResetToken, RedirectToLocal)
        // Bạn có thể copy phần đó từ các phản hồi trước nếu cần.
        // VÍ DỤ CHO FACEBOOK LOGIN (PHẦN TẠO USER):
        // var newUser = new User
        // {
        //     Username = newUsername,
        //     Email = model.Email,
        //     FullName = model.FullName ?? "Người dùng Facebook",
        //     FacebookId = model.FacebookUserId,
        //     Role = "User",
        //     CreatedAt = DateTime.Now,
        //     Password = GenerateRandomPasswordPlaceholder() // NẾU CỘT PASSWORD LÀ NOT NULL
        // };

        // VÍ DỤ CHO GOOGLE LOGIN (PHẦN TẠO USER):
        // var newUser = new User
        // {
        //     Username = newUsername,
        //     Email = email,
        //     FullName = fullName ?? "Người dùng Google",
        //     GoogleId = googleUserId,
        //     Role = "User",
        //     CreatedAt = DateTime.Now,
        //     Password = GenerateRandomPasswordPlaceholder(), // NẾU CỘT PASSWORD LÀ NOT NULL
        //     AvatarUrl = payload.Picture ?? "/images/default-avatar.png"
        // };


        // ============================================================
        // ĐĂNG NHẬP MẠNG XÃ HỘI (Facebook, Google) - Copy từ lần trước nếu bạn đã có
        // ============================================================
        // POST: Accounts/FacebookLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FacebookLogin(FacebookLoginViewModel model, string? returnUrl = null)
        {
            Console.WriteLine("AccountsController: FacebookLogin POST action called.");
            ViewData["ReturnUrl"] = returnUrl ?? string.Empty;

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                TempData["FacebookLoginError"] = "Dữ liệu đăng nhập Facebook không hợp lệ (ModelState): " + string.Join("; ", errors);
                Console.WriteLine($"FacebookLogin POST Error: ModelState is Invalid. Errors: {string.Join("; ", errors)}");
                return RedirectToAction("Login", new { returnUrl = ViewData["ReturnUrl"]?.ToString() });
            }
            Console.WriteLine("FacebookLogin POST: ModelState is Valid.");

            var facebookAppId = _configuration["Authentication:Facebook:AppId"];
            var facebookAppSecret = _configuration["Authentication:Facebook:AppSecret"];

            if (string.IsNullOrEmpty(facebookAppId) || string.IsNullOrEmpty(facebookAppSecret))
            {
                TempData["FacebookLoginError"] = "Cấu hình Facebook App ID/App Secret bị thiếu trên server.";
                Console.WriteLine("FacebookLogin POST Error: Facebook AppId or AppSecret is missing.");
                return RedirectToAction("Login", new { returnUrl = ViewData["ReturnUrl"]?.ToString() });
            }

            var httpClient = _httpClientFactory.CreateClient();
            var debugTokenUrl = $"https://graph.facebook.com/debug_token?input_token={model.AccessToken}&access_token={facebookAppId}|{facebookAppSecret}";

            try
            {
                var response = await httpClient.GetAsync(debugTokenUrl);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    TempData["FacebookLoginError"] = $"FB Token Validation Failed. Status: {response.StatusCode}. Details: {errorContent}";
                    Console.WriteLine($"FacebookLogin POST Error: FB Token Validation Failed. Status: {response.StatusCode}. Details: {errorContent}");
                    return RedirectToAction("Login", new { returnUrl = ViewData["ReturnUrl"]?.ToString() });
                }

                var content = await response.Content.ReadAsStringAsync();
                var tokenData = JObject.Parse(content)["data"];

                if (tokenData == null || tokenData["is_valid"]?.Value<bool>() != true ||
                    tokenData["app_id"]?.Value<string>() != facebookAppId ||
                    tokenData["user_id"]?.Value<string>() != model.FacebookUserId)
                {
                    TempData["FacebookLoginError"] = "Token Facebook không hợp lệ hoặc không khớp.";
                    Console.WriteLine($"FacebookLogin POST Error: Invalid FB Token or mismatch. Data: {tokenData?.ToString()}");
                    return RedirectToAction("Login", new { returnUrl = ViewData["ReturnUrl"]?.ToString() });
                }
                Console.WriteLine("FacebookLogin POST: Token validation successful.");
            }
            catch (Exception ex)
            {
                TempData["FacebookLoginError"] = "Lỗi khi xác thực token Facebook: " + ex.Message;
                Console.WriteLine($"FacebookLogin POST Exception during token validation: {ex.ToString()}");
                return RedirectToAction("Login", new { returnUrl = ViewData["ReturnUrl"]?.ToString() });
            }

            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.FacebookId == model.FacebookUserId);
                if (user != null)
                {
                    await SignInUser(user, false);
                }
                else
                {
                    if (!string.IsNullOrEmpty(model.Email))
                    {
                        var existingUserWithEmail = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                        if (existingUserWithEmail != null)
                        {
                            TempData["FacebookLoginError"] = $"Email {model.Email} đã được sử dụng bởi tài khoản khác.";
                            return RedirectToAction("Login", new { returnUrl = ViewData["ReturnUrl"]?.ToString() });
                        }
                    }
                    var newUsername = !string.IsNullOrEmpty(model.Email) ? model.Email : $"fb_{model.FacebookUserId}";
                    if (await _context.Users.AnyAsync(u => u.Username == newUsername))
                    {
                        newUsername = $"fb_{model.FacebookUserId}_{Guid.NewGuid().ToString("N").Substring(0, 6)}";
                    }
                    var newUser = new User
                    {
                        Username = newUsername,
                        Email = model.Email,
                        FullName = model.FullName ?? "Người dùng Facebook",
                        FacebookId = model.FacebookUserId,
                        Role = "User",
                        CreatedAt = DateTime.Now,
                        Password = GenerateRandomPasswordPlaceholder()
                    };
                    _context.Users.Add(newUser);
                    await _context.SaveChangesAsync();
                    await SignInUser(newUser, false);
                }
                return RedirectToLocal(ViewData["ReturnUrl"]?.ToString());
            }
            catch (DbUpdateException dbEx)
            {
                TempData["FacebookLoginError"] = "Lỗi lưu DB (FB): " + GetInnerExceptionMessages(dbEx);
                return RedirectToAction("Login", new { returnUrl = ViewData["ReturnUrl"]?.ToString() });
            }
            catch (Exception ex)
            {
                TempData["FacebookLoginError"] = "Lỗi không xác định (FB): " + ex.Message;
                return RedirectToAction("Login", new { returnUrl = ViewData["ReturnUrl"]?.ToString() });
            }
        }


        // POST: Accounts/GoogleLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GoogleLogin(GoogleLoginViewModel model, string? returnUrl = null)
        {
            Console.WriteLine("AccountsController: GoogleLogin POST action called.");
            ViewData["ReturnUrl"] = returnUrl ?? string.Empty;
            bool isOfflineDebugMode = false; // Đặt là false cho online

            if (!ModelState.IsValid || string.IsNullOrEmpty(model.IdToken))
            {
                TempData["GoogleLoginError"] = "ID Token Google bị thiếu.";
                return RedirectToAction("Login", new { returnUrl = ViewData["ReturnUrl"]?.ToString() });
            }

            try
            {
                var googleClientId = _configuration["Authentication:Google:ClientId"];
                if (string.IsNullOrEmpty(googleClientId))
                {
                    TempData["GoogleLoginError"] = "Google Client ID chưa được cấu hình.";
                    return RedirectToAction("Login", new { returnUrl = ViewData["ReturnUrl"]?.ToString() });
                }

                GoogleJsonWebSignature.Payload payload;
                if (isOfflineDebugMode)
                {
                    Console.WriteLine("GoogleLogin POST WARNING: OFFLINE DEBUG MODE - SKIPPING ID TOKEN SIGNATURE VALIDATION. THIS IS NOT SAFE FOR PRODUCTION!");
                    var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                    var jsonToken = handler.ReadJwtToken(model.IdToken);
                    payload = new GoogleJsonWebSignature.Payload
                    {
                        Subject = jsonToken.Claims.FirstOrDefault(claim => claim.Type == "sub")?.Value,
                        Email = jsonToken.Claims.FirstOrDefault(claim => claim.Type == "email")?.Value,
                        Name = jsonToken.Claims.FirstOrDefault(claim => claim.Type == "name")?.Value,
                        Picture = jsonToken.Claims.FirstOrDefault(claim => claim.Type == "picture")?.Value,
                        EmailVerified = bool.TryParse(jsonToken.Claims.FirstOrDefault(claim => claim.Type == "email_verified")?.Value, out var ev) ? ev : false,
                        Audience = jsonToken.Audiences?.ToList()
                    };
                    bool audienceMatch = false;
                    if (payload.Audience is string audString && audString == googleClientId) audienceMatch = true;
                    else if (payload.Audience is IList<string> audList && audList.Contains(googleClientId)) audienceMatch = true;
                    if (!audienceMatch) throw new InvalidJwtException($"Audience của ID Token không khớp Client ID ('{googleClientId}') (offline).");
                    Console.WriteLine($"GoogleLogin POST (Offline Mode): Parsed Payload. Google User ID (sub): {payload.Subject}, Email: {payload.Email}, Name: {payload.Name}");
                    Console.WriteLine("GoogleLogin POST (Offline Mode): Audience check passed.");
                }
                else
                {
                    try
                    {
                        var validationSettings = new GoogleJsonWebSignature.ValidationSettings { Audience = new[] { googleClientId } };
                        payload = await GoogleJsonWebSignature.ValidateAsync(model.IdToken, validationSettings);
                        Console.WriteLine($"GoogleLogin POST: ID Token validated online. Google User ID (sub): {payload.Subject}, Email: {payload.Email}, Name: {payload.Name}, EmailVerified: {payload.EmailVerified}");
                    }
                    catch (Exception ex)
                    {
                        TempData["GoogleLoginError"] = "ID Token Google không hợp lệ: " + GetInnerExceptionMessages(ex);
                        Console.WriteLine($"GoogleLogin POST Error: Invalid Google ID Token (Online Mode). Exception: {GetInnerExceptionMessages(ex)}");
                        return RedirectToAction("Login", new { returnUrl = ViewData["ReturnUrl"]?.ToString() });
                    }
                }

                var googleUserId = payload.Subject;
                if (string.IsNullOrEmpty(googleUserId))
                {
                    TempData["GoogleLoginError"] = "Không lấy được Google User ID.";
                    Console.WriteLine("GoogleLogin POST Error: Google User ID (sub) is missing from ID Token payload after parse/validate.");
                    return RedirectToAction("Login", new { returnUrl = ViewData["ReturnUrl"]?.ToString() });
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.GoogleId == googleUserId);
                if (user != null)
                {
                    Console.WriteLine($"GoogleLogin POST: User found with GoogleId '{googleUserId}'. Username: '{user.Username}'.");
                    await SignInUser(user, false);
                }
                else
                {
                    Console.WriteLine($"GoogleLogin POST: No user found with GoogleId '{googleUserId}'. Attempting to create new user.");
                    var email = payload.Email;
                    if (!string.IsNullOrEmpty(email))
                    {
                        var existingUserWithEmail = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                        if (existingUserWithEmail != null)
                        {
                            TempData["GoogleLoginError"] = $"Email {email} đã được sử dụng bởi tài khoản khác ({existingUserWithEmail.Username}).";
                            Console.WriteLine($"GoogleLogin POST Error: Email '{email}' already exists for another user '{existingUserWithEmail.Username}'.");
                            return RedirectToAction("Login", new { returnUrl = ViewData["ReturnUrl"]?.ToString() });
                        }
                    }
                    var newUsername = !string.IsNullOrEmpty(email) ? email : $"gg_{googleUserId}";
                    if (await _context.Users.AnyAsync(u => u.Username == newUsername))
                    {
                        newUsername = $"gg_{googleUserId}_{Guid.NewGuid().ToString("N").Substring(0, 6)}";
                    }
                    Console.WriteLine($"GoogleLogin POST: Final username for new Google user: '{newUsername}'.");

                    var newUser = new User
                    {
                        Username = newUsername,
                        Email = email,
                        FullName = payload.Name ?? "Người dùng Google",
                        GoogleId = googleUserId,
                        Role = "User",
                        CreatedAt = DateTime.Now,
                        Password = GenerateRandomPasswordPlaceholder(),
                        AvatarUrl = payload.Picture ?? "/images/default-avatar.png"
                    };
                    _context.Users.Add(newUser);
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"GoogleLogin POST: New user '{newUser.Username}' created successfully with UserId '{newUser.UserId}'.");
                    await SignInUser(newUser, false);
                }
                return RedirectToLocal(ViewData["ReturnUrl"]?.ToString());
            }
            catch (DbUpdateException dbEx)
            {
                TempData["GoogleLoginError"] = "Lỗi lưu DB (Google): " + GetInnerExceptionMessages(dbEx);
                Console.WriteLine($"GoogleLogin POST DbUpdateException: {GetInnerExceptionMessages(dbEx)}");
                return RedirectToAction("Login", new { returnUrl = ViewData["ReturnUrl"]?.ToString() });
            }
            catch (Exception ex)
            {
                TempData["GoogleLoginError"] = "Lỗi không xác định (Google): " + ex.Message;
                Console.WriteLine($"GoogleLogin POST General Exception: {ex.ToString()}");
                return RedirectToAction("Login", new { returnUrl = ViewData["ReturnUrl"]?.ToString() });
            }
        }


        // GET: Accounts/Logout
        public async Task<IActionResult> Logout()
        {
            Console.WriteLine("AccountsController: Logout action called.");
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            Console.WriteLine("Logout: User signed out.");
            return RedirectToAction("Login", "Accounts");
        }


        private async Task SignInUser(User user, bool isPersistent)
        {
            Console.WriteLine($"SignInUser: Attempting to sign in user '{user.Username}'. IsPersistent: {isPersistent}. UserId: {user.UserId}");
            try
            {
                user.LastLoginAt = DateTime.Now;
                _context.Users.Update(user);
                Console.WriteLine($"SignInUser: Attempting to save LastLoginAt for user '{user.Username}'.");
                await _context.SaveChangesAsync();
                Console.WriteLine($"SignInUser: Updated LastLoginAt for user '{user.Username}'.");
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine($"SignInUser DbUpdateException while saving LastLoginAt: {dbEx.ToString()}");
                string innerErrorMessages = GetInnerExceptionMessages(dbEx);
                Console.WriteLine("SignInUser Inner Exception Details for LastLoginAt: " + innerErrorMessages);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SignInUser General Exception while updating LastLoginAt: {ex.ToString()}");
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Role, user.Role)
            };
            Console.WriteLine($"SignInUser: Created claims for user '{user.Username}'.");
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties { IsPersistent = isPersistent };
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
            Console.WriteLine($"SignInUser: User '{user.Username}' successfully signed in with cookie.");
        }

        private string GetInnerExceptionMessages(Exception ex)
        {
            string messages = ex.Message;
            Exception? inner = ex.InnerException;
            int i = 0;
            while (inner != null)
            {
                messages += $" | Inner ({i++}): {inner.Message}";
                inner = inner.InnerException;
            }
            return messages;
        }

        private string GenerateRandomPasswordPlaceholder(int length = 32)
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()-_=+[]{}|;:,.<>/?";
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                var byteBuffer = new byte[length];
                rng.GetBytes(byteBuffer);
                var chars = new char[length];
                for (int i = 0; i < length; i++)
                {
                    chars[i] = validChars[byteBuffer[i] % validChars.Length];
                }
                return new string(chars);
            }
        }

        private string GenerateSecurePasswordResetToken()
        {
            byte[] tokenBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(tokenBytes);
            }
            return Convert.ToBase64String(tokenBytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        // Helper để chuyển hướng an toàn
        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                Console.WriteLine($"RedirectToLocal: Redirecting to local ReturnUrl: {returnUrl}");
                return Redirect(returnUrl);
            }
            else
            {
                Console.WriteLine("RedirectToLocal: returnUrl is not local or is empty. Redirecting to Home/Index.");
                return RedirectToAction("Index", "Home");
            }
        }
    }
}