document.addEventListener("DOMContentLoaded", function () {
  // Tabs Navigation - Phiên bản đã sửa
  const tabButtons = document.querySelectorAll(".tab-list button");

  console.log("Script quản lý bình luận đã được khởi tạo");
  console.log("Số lượng nút tab tìm thấy:", tabButtons.length);

  tabButtons.forEach((button) => {
    button.addEventListener("click", function (e) {
      e.preventDefault();

      console.log("Tab button clicked:", this.id); // Debug để kiểm tra event có hoạt động

      // 1. Bỏ active khỏi tất cả các tab
      tabButtons.forEach((tab) => {
        tab.classList.remove("active");
        tab.setAttribute("aria-selected", "false");
      });

      // 2. Ẩn tất cả các tab content
      document.querySelectorAll(".tab-pane").forEach((pane) => {
        pane.classList.remove("show", "active");
      });

      // 3. Active tab hiện tại
      this.classList.add("active");
      this.setAttribute("aria-selected", "true");

      // 4. Hiển thị nội dung tab tương ứng
      const targetId = this.getAttribute("data-bs-target").substring(1);
      const targetPane = document.getElementById(targetId);
      console.log("Target pane:", targetId); // Debug
      if (targetPane) {
        targetPane.classList.add("show", "active");
      } else {
        console.error("Tab content not found:", targetId);
      }
    });
  });

  // Modal elements
  const deleteModal = document.getElementById("deleteModal");
  const commentModal = document.getElementById("commentModal");
  const confirmDeleteButton = document.getElementById("confirmDelete");
  const cancelDeleteButton = document.getElementById("cancelDelete");

  // Lấy tất cả các nút đóng modal
  const modalCloseButtons = document.querySelectorAll(
    ".modal-close, #cancelDelete, .modal-close-btn"
  );

  let itemToDeleteId = null;
  let itemToDeleteType = null;

  // --- REVIEWS FILTERING LOGIC ---
  const reviewSearchInput = document.getElementById("reviewSearchInput");
  const reviewRatingFilter = document.getElementById("reviewRatingFilter");
  const applyReviewFiltersBtn = document.getElementById(
    "applyReviewFiltersBtn"
  );

  applyReviewFiltersBtn.addEventListener("click", function () {
    const searchText = reviewSearchInput.value.toLowerCase();
    const selectedRating = reviewRatingFilter.value;
    const tableRows = document.querySelectorAll("#reviewsTable tbody tr");

    tableRows.forEach((row) => {
      if (row.querySelector('td[colspan="7"]')) {
        // Skip "no data" row
        row.style.display = "";
        return;
      }

      const userName =
        row.cells[0].querySelector(".user-name")?.textContent.toLowerCase() ||
        "";
      const userEmail =
        row.cells[0].querySelector(".user-email")?.textContent.toLowerCase() ||
        "";
      const contentData =
        row.cells[1]
          .querySelector(".comment-content")
          ?.textContent.toLowerCase() || "";
      const rowRating = row.dataset.rating;

      let show = true;

      // Search text filter
      if (
        searchText &&
        !(
          userName.includes(searchText) ||
          userEmail.includes(searchText) ||
          contentData.includes(searchText)
        )
      ) {
        show = false;
      }
      // Rating filter
      if (selectedRating !== "all" && rowRating !== selectedRating) {
        show = false;
      }

      row.style.display = show ? "" : "none";
    });
  });

  // --- COMMENTS FILTERING LOGIC ---
  const commentSearchInput = document.getElementById("commentSearchInput");
  const commentTypeFilter = document.getElementById("commentTypeFilter");
  const applyCommentFiltersBtn = document.getElementById(
    "applyCommentFiltersBtn"
  );

  applyCommentFiltersBtn.addEventListener("click", function () {
    const searchText = commentSearchInput.value.toLowerCase();
    const selectedType = commentTypeFilter.value;
    const tableRows = document.querySelectorAll("#commentsTable tbody tr");

    tableRows.forEach((row) => {
      if (row.querySelector('td[colspan="7"]')) {
        // Skip "no data" row
        row.style.display = "";
        return;
      }

      const userName =
        row.cells[0].querySelector(".user-name")?.textContent.toLowerCase() ||
        "";
      const userEmail =
        row.cells[0].querySelector(".user-email")?.textContent.toLowerCase() ||
        "";
      const contentData =
        row.cells[1]
          .querySelector(".comment-content")
          ?.textContent.toLowerCase() || "";
      const rowPostType = row.dataset.postType;

      let show = true;

      // Search text filter
      if (
        searchText &&
        !(
          userName.includes(searchText) ||
          userEmail.includes(searchText) ||
          contentData.includes(searchText)
        )
      ) {
        show = false;
      }
      // Post type filter
      if (selectedType !== "all" && rowPostType !== selectedType) {
        show = false;
      }

      row.style.display = show ? "" : "none";
    });
  });

  // --- VIEW DETAIL MODAL LOGIC ---
  document.querySelectorAll(".view-btn-trigger").forEach((button) => {
    button.addEventListener("click", function (e) {
      e.preventDefault();

      // Lấy dữ liệu từ thuộc tính data-
      const id = this.getAttribute("data-id");
      const userName = this.getAttribute("data-user-name");
      const userEmail = this.getAttribute("data-user-email");
      const userAvatar = this.getAttribute("data-user-avatar");
      const itemTitle = this.getAttribute("data-item-title");
      const itemUrl = this.getAttribute("data-item-url");
      const content = this.getAttribute("data-content");
      const rating = this.getAttribute("data-rating");
      const createdAt = this.getAttribute("data-created-at");
      const itemType = this.getAttribute("data-item-type");
      const itemTypeDetail = this.getAttribute("data-item-type-detail");
      const imageUrl = this.getAttribute("data-image-url");

      // Cập nhật nội dung modal
      document.querySelector("#modal-user-avatar").src = userAvatar;
      document.querySelector(".user-name").textContent = userName;
      document.querySelector(".user-email").textContent = userEmail;
      document.querySelector(".comment-post-title").textContent = itemTitle;
      document.querySelector(".comment-post-link").href = itemUrl;
      document.querySelector(".comment-post-type-detail").textContent =
        itemTypeDetail ? `(${itemTypeDetail})` : "";
      document.querySelector(".comment-body-content").innerHTML = content;
      document.querySelector(".date-value").textContent = createdAt;

      // Hiển thị/ẩn rating dựa vào loại item
      const ratingDisplay = document.querySelector(".rating-display");
      if (itemType === "Review" && rating) {
        ratingDisplay.style.display = "block";

        // Tạo stars
        const starsContainer = document.querySelector(".stars-container");
        starsContainer.innerHTML = "";

        const ratingValue = parseFloat(rating);
        for (let i = 1; i <= 5; i++) {
          const starIcon = document.createElement("i");
          if (i <= ratingValue) {
            starIcon.className = "fas fa-star text-warning";
          } else if (i - 0.5 === ratingValue) {
            starIcon.className = "fas fa-star-half-alt text-warning";
          } else {
            starIcon.className = "far fa-star text-warning";
          }
          starsContainer.appendChild(starIcon);
        }

        document.querySelector(".rating-value").textContent =
          ratingValue.toFixed(1);
      } else {
        ratingDisplay.style.display = "none";
      }

      // Hiển thị/ẩn ảnh nếu có
      const imageContainer = document.querySelector(".comment-image-container");
      const modalImage = document.querySelector(".modal-comment-image");

      if (
        imageUrl &&
        imageUrl !== "/images/default-postImage.png" &&
        imageUrl.startsWith("/")
      ) {
        imageContainer.style.display = "block";
        modalImage.src = imageUrl;
        modalImage.alt =
          itemType === "Review" ? "Ảnh đánh giá" : "Ảnh bình luận";
      } else {
        imageContainer.style.display = "none";
      }

      // Hiển thị modal
      commentModal.style.display = "block";
    });
  });

  // --- DELETE MODAL LOGIC ---
  document.querySelectorAll(".delete-btn-trigger").forEach((button) => {
    button.addEventListener("click", function (e) {
      e.preventDefault();

      // Lấy ID và loại của item cần xóa
      itemToDeleteId = this.getAttribute("data-id");
      itemToDeleteType = this.getAttribute("data-item-type");

      // Cập nhật thông báo xác nhận
      const deleteMessage =
        itemToDeleteType === "Review"
          ? "Bạn có chắc chắn muốn xóa đánh giá này?"
          : "Bạn có chắc chắn muốn xóa bình luận này?";

      document.getElementById("deleteModalMessage").textContent = deleteMessage;

      // Hiển thị modal xóa
      deleteModal.style.display = "block";
    });
  });

  // Xử lý xác nhận xóa
  confirmDeleteButton.addEventListener("click", function () {
    if (itemToDeleteId && itemToDeleteType) {
      // Gửi request AJAX để xóa item
      const token = document.querySelector(
        'input[name="__RequestVerificationToken"]'
      ).value;

      fetch("/Admin/DeleteComment", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          RequestVerificationToken: token,
        },
        body: JSON.stringify({
          id: itemToDeleteId,
          type: itemToDeleteType,
        }),
      })
        .then((response) => response.json())
        .then((data) => {
          if (data.success) {
            // Xóa dòng tương ứng trên bảng
            const rows = document.querySelectorAll(
              `[data-id="${itemToDeleteId}"]`
            );
            rows.forEach((r) => {
              const parentRow = r.closest("tr");
              if (parentRow) {
                parentRow.remove();
              }
            });

            // Kiểm tra nếu không còn dòng nào, hiển thị thông báo "không có dữ liệu"
            const reviewsTable = document.getElementById("reviewsTable");
            const commentsTable = document.getElementById("commentsTable");

            if (itemToDeleteType === "Review") {
              const remainingReviews = reviewsTable.querySelectorAll(
                'tbody tr:not([style*="display: none"])'
              );
              if (remainingReviews.length === 0) {
                const noDataRow = document.createElement("tr");
                noDataRow.innerHTML =
                  '<td colspan="7" class="text-center" style="padding: 20px;">Không có đánh giá nào.</td>';
                reviewsTable.querySelector("tbody").appendChild(noDataRow);
              }
            } else {
              const remainingComments = commentsTable.querySelectorAll(
                'tbody tr:not([style*="display: none"])'
              );
              if (remainingComments.length === 0) {
                const noDataRow = document.createElement("tr");
                noDataRow.innerHTML =
                  '<td colspan="7" class="text-center" style="padding: 20px;">Không có bình luận nào.</td>';
                commentsTable.querySelector("tbody").appendChild(noDataRow);
              }
            }

            // Hiển thị thông báo thành công
            showToast("Xóa thành công", "success");
          } else {
            // Hiển thị thông báo lỗi
            showToast(
              "Có lỗi xảy ra: " + (data.message || "Không thể xóa mục này"),
              "error"
            );
          }

          // Đóng modal
          deleteModal.style.display = "none";
        })
        .catch((error) => {
          console.error("Error:", error);
          showToast("Có lỗi xảy ra khi xóa mục", "error");
          deleteModal.style.display = "none";
        });
    }
  });

  // --- MODAL CONTROL LOGIC ---

  // Đóng các modal khi click vào nút đóng
  modalCloseButtons.forEach((button) => {
    button.addEventListener("click", function () {
      commentModal.style.display = "none";
      deleteModal.style.display = "none";
    });
  });

  // Đóng modal khi click ra ngoài
  window.addEventListener("click", function (e) {
    if (e.target === commentModal) {
      commentModal.style.display = "none";
    }
    if (e.target === deleteModal) {
      deleteModal.style.display = "none";
    }
  });

  // Hàm hiển thị thông báo toast
  function showToast(message, type) {
    // Kiểm tra nếu đã có container toast
    let toastContainer = document.querySelector(".toast-container");
    if (!toastContainer) {
      toastContainer = document.createElement("div");
      toastContainer.className = "toast-container";
      document.body.appendChild(toastContainer);
    }

    // Tạo toast mới
    const toast = document.createElement("div");
    toast.className = `toast ${type}`;
    toast.innerHTML = `
          <div class="toast-content">
              <i class="${
                type === "success"
                  ? "fas fa-check-circle"
                  : "fas fa-exclamation-circle"
              }"></i>
              <span>${message}</span>
          </div>
          <button class="toast-close">&times;</button>
      `;

    // Thêm toast vào container
    toastContainer.appendChild(toast);

    // Hiển thị toast với animation
    setTimeout(() => {
      toast.classList.add("show");
    }, 10);

    // Tự động đóng toast sau 3 giây
    setTimeout(() => {
      toast.classList.remove("show");
      setTimeout(() => {
        toast.remove();
      }, 300);
    }, 3000);

    // Xử lý nút đóng toast
    toast.querySelector(".toast-close").addEventListener("click", function () {
      toast.classList.remove("show");
      setTimeout(() => {
        toast.remove();
      }, 300);
    });
  }
});
