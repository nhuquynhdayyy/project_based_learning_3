// In Models/ViewModels/AdminCommentViewModel.cs
using System;

namespace TourismWeb.Models.ViewModels
{
    public class AdminCommentViewModel
    {
        public int Id { get; set; }
        public string ItemType { get; set; } // "Review" or "PostComment"
        public string UserFullName { get; set; }
        public string UserEmail { get; set; }
        public string UserAvatar { get; set; }
        public string Content { get; set; }
        public int? RelatedItemId { get; set; }
        public string RelatedItemTitle { get; set; }
        public string RelatedItemController { get; set; }
        public string RelatedItemTypeDetail { get; set; } // THÊM DÒNG NÀY: VD: "Cẩm nang (Bình luận)", "Địa điểm du lịch (Đánh giá)"
        public int? Rating { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ImageUrl { get; set; }

        public string DeleteControllerName => ItemType == "Review" ? "Reviews" : "PostComments";
        // public string DeleteActionName => "DeleteConfirmed"; // Sẽ dùng Delete và id trong route
    }
}