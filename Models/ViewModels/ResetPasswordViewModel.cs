using System.ComponentModel.DataAnnotations;
// Dòng using TourismWeb.Models.ViewModels; ở đây là THỪA và có thể gây lỗi nếu nó là chính namespace hiện tại.
// Thông thường, bạn không cần 'using' chính namespace mà class đó đang được định nghĩa.
// Hãy thử bỏ dòng này đi nếu TourismWeb.Models.ViewModels chính là namespace bạn đang khai báo bên dưới.

namespace TourismWeb.Models.ViewModels // <<--- ĐẢM BẢO LÀ NAMESPACE NÀY
{
    public class ResetPasswordViewModel
    {
        [Required]
        public string Token { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới.")]
        [StringLength(100, ErrorMessage = "{0} phải có ít nhất {2} và tối đa {1} ký tự.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới")]
        public string Password { get; set; } = null!;

        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu mới")]
        [Compare("Password", ErrorMessage = "Mật khẩu mới và mật khẩu xác nhận không khớp.")]
        public string ConfirmPassword { get; set; } = null!;
    }
}