using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace TourismWeb.Models
{
    public enum ReportType
    {
        [Display(Name = "Spam")]
        Spam,
        [Display(Name = "Nội dung không phù hợp")]
        InappropriateContent,
        [Display(Name = "Thông tin không chính xác")]
        IncorrectInformation,
        [Display(Name = "Vi phạm bản quyền")]
        CopyrightInfringement,
        [Display(Name = "Quấy rối")]
        Harassment,
        [Display(Name = "Ngôn từ thù ghét")]
        HateSpeech,
        [Display(Name = "Khác")]
        Other
    }

    public enum ReportTargetType
    {
        [Display(Name = "Bài viết")]
        Post,
        [Display(Name = "Bình luận")]
        Comment,
        [Display(Name = "Đánh giá")]
        Review,
        [Display(Name = "Người dùng")]
        User,
        [Display(Name = "Địa điểm")]
        Spot
    }

    public enum ReportStatus
    {
        [Display(Name = "Chờ xử lý")]
        Pending,
        [Display(Name = "Đã xử lý")]
        Resolved,
        [Display(Name = "Đã bỏ qua")]
        Dismissed
    }

    public class Report
    {
        [Key]
        public int ReportId { get; set; }

        public int ReporterUserId { get; set; }
        [ValidateNever] // ✅ Không kiểm tra từ form
        [ForeignKey("ReporterUserId")]
        public virtual User ReporterUser { get; set; } // Người báo cáo

        [Required]
        public ReportType? TypeOfReport { get; set; }

        [Required]
        public ReportTargetType? TargetType { get; set; }
        public int? TargetId { get; set; } // ID của Post, Comment, User, Spot...

        public int? ReportedUserId { get; set; } // ID của người dùng bị báo cáo/sở hữu nội dung
        [ValidateNever] // ✅ Không kiểm tra từ form
        [ForeignKey("ReportedUserId")]
        public virtual User ReportedUser { get; set; }

        [Required]
        [StringLength(1000)]
        public string Reason { get; set; }

        public DateTime ReportedAt { get; set; } = DateTime.Now;

        public ReportStatus Status { get; set; } = ReportStatus.Pending;

        public int? AdminUserId { get; set; } // ID của admin xử lý (cần User ID của admin hiện tại)
        [ValidateNever] // ✅ Không kiểm tra từ form
        [ForeignKey("AdminUserId")]
        public virtual User AdminUser { get; set; } // Người dùng Admin đã xử lý

        public DateTime? ResolvedAt { get; set; }
        [StringLength(500)]
        [ValidateNever] // Ngăn ModelState kiểm tra khi không có trong form
        public string? AdminNotes { get; set; }
    }
}