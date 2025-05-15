using System.ComponentModel.DataAnnotations;

namespace TourismWeb.Models.ViewModels // <<--- ĐẢM BẢO LÀ NAMESPACE NÀY
{
    public class FacebookLoginViewModel
    {
        [Required]
        public string FacebookUserId { get; set; } = null!;
        public string? Email { get; set; }
        public string? FullName { get; set; }
        [Required]
        public string AccessToken { get; set; } = null!;
    }
}