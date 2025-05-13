namespace TourismWeb.Models.ViewModels 
{
    public class ReviewViewModel
    {
        // Các thuộc tính này được chọn để phù hợp với những gì bạn muốn hiển thị trong phần "Đánh giá của mọi người"

        public string AvatarImageUrl { get; set; } // Đường dẫn đến ảnh đại diện của người đánh giá

        public int Rating { get; set; } // Số sao đánh giá (ví dụ: 1 đến 5)

        public string Comment { get; set; } // Nội dung bình luận

        public string AuthorName { get; set; } // Tên của người đánh giá

        public string TourName { get; set; } // Tên của tour hoặc địa điểm được đánh giá

        // Bạn có thể thêm các thuộc tính khác nếu cần cho view, ví dụ:
        public DateTime ReviewDate { get; set; }
        // public int ReviewId { get; set; } // Nếu bạn cần ID của review trong view
    }
}