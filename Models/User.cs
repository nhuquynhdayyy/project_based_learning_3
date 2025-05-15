using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations; // Đảm bảo bạn có using này

namespace TourismWeb.Models // Hoặc namespace model của bạn
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ và tên.")] // Thêm ErrorMessage nếu muốn
        [StringLength(100)]
        public string FullName { get; set; } = null!; // Khởi tạo để tránh warning CS8618 nếu không gán trong constructor

        [DataType(DataType.Date)]
        [Display(Name = "Ngày sinh")]
        public DateTime? DateOfBirth { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")] // Thêm ErrorMessage
        [Display(Name = "Số điện thoại")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ email.")]
        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ.")]
        public string Email { get; set; } = null!; // Khởi tạo

        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Tên đăng nhập phải có từ 3 đến 50 ký tự.")]
        [Display(Name = "Tên đăng nhập")]
        public string Username { get; set; } = null!; // Khởi tạo

        public string? FacebookId { get; set; }
        public string? GoogleId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!; // Khởi tạo (Sẽ được hash)

        [Display(Name = "Ảnh đại diện")]
        public string AvatarUrl { get; set; } = "/images/default-avatar.png";

        public string Role { get; set; } = "User";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Đăng nhập lần cuối")]
        public DateTime? LastLoginAt { get; set; }

        [Display(Name = "Tiểu sử")]
        [StringLength(500, ErrorMessage = "Tiểu sử không được vượt quá 500 ký tự.")]
        public string? Bio { get; set; }

        [Display(Name = "Trạng thái người dùng")]
        public string UserStatus { get; set; } = "Hoạt động";

        // === CÁC THUỘC TÍNH MỚI CHO QUÊN MẬT KHẨU ===
        [StringLength(256)] // Độ dài tùy thuộc vào cách bạn tạo token
        public string? PasswordResetToken { get; set; }

        public DateTime? PasswordResetTokenExpiry { get; set; }
        // ===============================================

        // Các ICollection của bạn (giữ nguyên)
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Post> Posts { get; set; } = new List<Post>();
        public ICollection<PostComment> PostComments { get; set; } = new List<PostComment>();
        public ICollection<SpotFavorite> SpotFavorites { get; set; } = new List<SpotFavorite>();
        public ICollection<PostFavorite> PostFavorites { get; set; } = new List<PostFavorite>();
        public ICollection<SpotShare> SpotShares { get; set; } = new List<SpotShare>();
        public ICollection<PostShare> PostShares { get; set; } = new List<PostShare>();
        public ICollection<SpotImage> SpotImages { get; set; } = new List<SpotImage>();
        public ICollection<PostImage> PostImages { get; set; } = new List<PostImage>();
    }
}