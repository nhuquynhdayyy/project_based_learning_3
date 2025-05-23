using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace TourismWeb.Models
{
    public class Post
    {
        // --- Thuộc tính hiện có ---
        [Key]
        public int PostId { get; set; }

        [Required]
        public int UserId { get; set; }
        [ValidateNever]
        public User User { get; set; } // Đảm bảo model User có các thuộc tính như FullName, Bio?

        [Required]
        [Display(Name = "Địa điểm")] // "Spot" -> "Địa điểm" cho dễ hiểu hơn
        public int SpotId { get; set; }
        [ValidateNever]
        public TouristSpot Spot { get; set; } // Đảm bảo TouristSpot có các thuộc tính cần thiết

        [Required]
        [MaxLength(50)]
        // Xem xét dùng Enum để an toàn kiểu dữ liệu hơn?
        // public PostType TypeOfPost { get; set; }
        [RegularExpression("^(Địa điểm|Cẩm nang|Trải nghiệm|Bài viết)$", ErrorMessage = "Loại bài viết không hợp lệ.")] // Thêm "Bài viết" phòng trường hợp cần
        public string TypeOfPost { get; set; } // Loại bài viết

        [Required, MaxLength(100)]
        public string Title { get; set; } // Tiêu đề

        [Required]
        [DataType(DataType.MultilineText)]
        public string Content { get; set; } // Nội dung chính/Mô tả/Câu chuyện

        public string ImageUrl { get; set; } = "/images/default-postImage.png"; // Ảnh đại diện
        public DateTime CreatedAt { get; set; } = DateTime.Now; // Ngày tạo

        [Display(Name = "Trạng thái")] // "Status" -> "Trạng thái"
        public PostStatus Status { get; set; } = PostStatus.Pending; // Trạng thái duyệt bài

        // --- Thuộc tính MỚI cho Nội dung Động ---

        // == Cho Loại: "Địa điểm" ==
        [Display(Name = "Thời gian tham quan ước tính")]
        public string? EstimatedVisitTime { get; set; } // ví dụ: "2-3 giờ", "Nửa ngày"

        [Display(Name = "Thông tin giá vé")]
        public string? TicketPriceInfo { get; set; }    // ví dụ: "Miễn phí", "Từ 50.000 VND", "Người lớn: 100k, Trẻ em: 50k"

        [Display(Name = "Điểm đánh giá (trên 5)")]
        [Range(0, 5)]
        public double? LocationRating { get; set; }      // ví dụ: 4.5

        [Display(Name = "Lịch trình gợi ý")]
        public string? SuggestedItinerary { get; set; } // Có thể lưu HTML/Markdown đơn giản hoặc văn bản thuần

        // == Cho Loại: "Cẩm nang" ==
        [Display(Name = "Tóm tắt/Giới thiệu cẩm nang")]
        public string? GuidebookSummary { get; set; }

        [Display(Name = "Mẹo du lịch")]
        public string? TravelTips { get; set; }         // Lưu dưới dạng danh sách (HTML <ul><li>) hoặc Markdown

        [Display(Name = "Gợi ý đồ mang theo")]
        public string? PackingListSuggestions { get; set; } // Lưu dưới dạng văn bản/HTML/Markdown có cấu trúc

        [Display(Name = "Chi phí tham khảo")]
        public string? EstimatedCosts { get; set; }     // Lưu dưới dạng văn bản/HTML/Markdown có cấu trúc

        // Xem xét một thực thể liên quan riêng `DocumentLink` nếu bạn cần các liên kết có cấu trúc
        [Display(Name = "Tài liệu hữu ích (Links)")]
        public string? UsefulDocumentsHtml { get; set; } // Lưu dưới dạng thẻ HTML <a>

        // == Cho Loại: "Trải nghiệm" ==
        [Display(Name = "Ngày kết thúc trải nghiệm")]
        public DateTime? ExperienceEndDate { get; set; } // Để tính thời gian kéo dài

        [Display(Name = "Người đồng hành")]
        public string? Companions { get; set; }         // ví dụ: "Gia đình", "Bạn bè", "Một mình"

        [Display(Name = "Chi phí ước tính")]
        public string? ApproximateCost { get; set; }    // ví dụ: "Khoảng 5 triệu VND/người"

        [Display(Name = "Đánh giá tổng quan (trên 10)")]
        [Range(0, 10)]
        public double? OverallExperienceRating { get; set; } // ví dụ: 9.0

        // Đối với đánh giá chi tiết, sử dụng các trường riêng biệt sẽ rõ ràng hơn là phân tích một chuỗi
        [Display(Name = "Điểm cảnh quan (trên 5)")]
        [Range(0, 5)]
        public double? RatingLandscape { get; set; }

        [Display(Name = "Điểm ẩm thực (trên 5)")]
        [Range(0, 5)]
        public double? RatingFood { get; set; }

        [Display(Name = "Điểm dịch vụ (trên 5)")]
        [Range(0, 5)]
        public double? RatingService { get; set; }

        [Display(Name = "Điểm giá cả (trên 5)")]
        [Range(0, 5)]
        public double? RatingPrice { get; set; }

        [Display(Name = "Những điểm nổi bật/Khoảnh khắc")]
        public string? ExperienceHighlights { get; set; } // Lưu dưới dạng danh sách (HTML <ul><li>) hoặc Markdown

        [Display(Name = "Tóm tắt hành trình")]
        public string? ExperienceItinerarySummary { get; set; } // Lưu dưới dạng văn bản/HTML/Markdown có cấu trúc

        [Display(Name = "Lời khuyên")]
        public string? Advice { get; set; }

        // --- Các Collection Hiện có ---
        // Sử dụng chúng tích cực hơn!
        public ICollection<PostImage> Images { get; set; } = new List<PostImage>(); // Cho các hình ảnh bổ sung
        public ICollection<PostFavorite> PostFavorites { get; set; } = new List<PostFavorite>(); // Yêu thích
        public ICollection<PostComment> Comments { get; set; } = new List<PostComment>(); // Bình luận
        public ICollection<PostShare> Shares { get; set; } = new List<PostShare>(); // Lượt chia sẻ

        // --- Có thể xem xét thêm ---
        // public ICollection<RelatedPost> RelatedPosts { get; set; } // Cần logic để điền dữ liệu
    }

    // Enum cho trạng thái bài viết
    public enum PostStatus
    {
        Pending, // Chờ duyệt
        Approved, // Đã duyệt
        Rejected // Bị từ chối
    }

    // Ví dụ Enum cho PostType (Tùy chọn nhưng được khuyến nghị)
    // public enum PostType
    // {
    //     DiaDiem, // Hoặc Location
    //     CamNang, // Hoặc Guidebook
    //     TraiNghiem, // Hoặc Experience
    //     BaiViet // Hoặc Article
    // }
}