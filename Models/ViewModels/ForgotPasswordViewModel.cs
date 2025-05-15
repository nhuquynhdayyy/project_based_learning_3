// File: Models/ViewModels/ForgotPasswordViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace TourismWeb.Models.ViewModels // <<--- ĐẢM BẢO DÒNG NÀY ĐÚNG
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ email.")]
        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ.")]
        public string Email { get; set; } = null!;
    }
}