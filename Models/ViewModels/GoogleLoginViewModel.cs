using System.ComponentModel.DataAnnotations;

namespace TourismWeb.Models.ViewModels // <<--- ĐẢM BẢO LÀ NAMESPACE NÀY
{
    public class GoogleLoginViewModel
    {
        [Required]
        public string IdToken { get; set; } = null!;
    }
}