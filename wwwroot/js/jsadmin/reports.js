document.addEventListener("DOMContentLoaded", function () {
  const reportModal = document.getElementById("reportModal");
  const closeModalButtons = document.querySelectorAll(
    ".modal-close, .modal-close-btn"
  );
  const filterForm = document.getElementById("filterForm");
  const mainStatusFilter = document.getElementById("report-status-filter-main");
  const hiddenStatusFilterInput = document.getElementById("hiddenStatusFilter");
  const pageSizeSelect = document.getElementById("pageSizeSelect");
  const processReportForm = document.getElementById("processReportForm"); // Lấy form xử lý report
  // --- THÊM MỚI: TỰ ĐỘNG CẬP NHẬT TRẠNG THÁI KHI CHỌN HÀNH ĐỘNG ---
  const modalNewStatus = document.getElementById("modalNewStatus");
  const adminActionRadios = document.querySelectorAll(
    'input[name="adminAction"]'
  );

  adminActionRadios.forEach((radio) => {
    radio.addEventListener("change", function () {
      // Nếu hành động là một hành động xử lý thực sự (không phải "ignore")
      // thì tự động chuyển trạng thái sang "Resolved"
      if (
        this.value === "delete_content" ||
        this.value === "warn_user" ||
        this.value === "ban_user"
      ) {
        if (modalNewStatus) {
          modalNewStatus.value = "Resolved";
        }
      }
      // Nếu chọn lại "Chỉ cập nhật trạng thái", chúng ta không làm gì cả,
      // để admin tự quyết định sẽ chuyển sang "Dismissed" hay giữ "Pending".
    });
  });

  // --- Xử lý Filter ---
  if (mainStatusFilter && filterForm && hiddenStatusFilterInput) {
    mainStatusFilter.addEventListener("change", function () {
      hiddenStatusFilterInput.value = this.value;
      // Đảm bảo pageNumber reset về 1 khi đổi status filter
      const pageNumberInput = filterForm.querySelector(
        'input[name="pageNumber"]'
      );
      if (pageNumberInput) pageNumberInput.value = "1";
      filterForm.requestSubmit(); // Submit form để lọc
    });
  }

  if (pageSizeSelect && filterForm) {
    pageSizeSelect.addEventListener("change", function () {
      const pageSizeInput = filterForm.querySelector('input[name="pageSize"]');
      if (pageSizeInput) {
        pageSizeInput.value = this.value;
      } else {
        let newInput = document.createElement("input");
        newInput.type = "hidden";
        newInput.name = "pageSize";
        newInput.value = this.value;
        filterForm.appendChild(newInput);
      }
      const pageNumberInput = filterForm.querySelector(
        'input[name="pageNumber"]'
      );
      if (pageNumberInput) pageNumberInput.value = "1";
      filterForm.requestSubmit();
    });
  }

  document.querySelectorAll(".pagination-btn[data-page]").forEach((button) => {
    button.addEventListener("click", function (e) {
      e.preventDefault();
      if (this.classList.contains("disabled")) return;

      const page = this.dataset.page;
      const pageNumberInput = filterForm.querySelector(
        'input[name="pageNumber"]'
      );
      if (pageNumberInput) {
        pageNumberInput.value = page;
        filterForm.requestSubmit();
      }
    });
  });

  // --- Xử lý Modal ---
  function openModal() {
    if (reportModal) reportModal.style.display = "block";
  }

  function closeModal() {
    if (reportModal) reportModal.style.display = "none";
  }

  closeModalButtons.forEach((button) => {
    button.addEventListener("click", closeModal);
  });

  window.addEventListener("click", (event) => {
    if (event.target === reportModal) {
      closeModal();
    }
  });

  // Mở modal khi click nút "Xử lý" (class "process-btn")
  document.querySelectorAll(".process-btn").forEach((button) => {
    button.addEventListener("click", function () {
      const reportId = this.dataset.reportId;
      fetchReportDetails(reportId);
    });
  });

  function fetchReportDetails(reportId) {
    // TODO: Hiển thị loading indicator nếu có
    fetch(`/Admin/GetReportDetails/${reportId}`) // Đảm bảo URL này đúng
      .then((response) => {
        if (!response.ok) {
          throw new Error("Network response was not ok " + response.statusText);
        }
        return response.json();
      })
      .then((data) => {
        populateModal(data);
        openModal();
      })
      .catch((error) => {
        console.error("Error fetching report details:", error);
        alert("Không thể tải chi tiết báo cáo. Vui lòng thử lại.");
      });
  }

  function populateModal(data) {
    document.getElementById("modalReportId").textContent = data.reportId;
    document.getElementById("modalHiddenReportId").value = data.reportId;
    // reportedUserIdToActOn được set trong form, nhưng controller hiện không dùng nhiều,
    // nếu controller dùng thì giá trị này quan trọng
    document.getElementById("modalHiddenReportedUserId").value =
      data.reportedUserId || "";

    // Reporter Info
    let reporterHtml = "N/A";
    if (data.reporterName) {
      reporterHtml = `
                <div class="user-cell">
                    <div class="user-avatar">
                        <img src="${data.reporterAvatar}" alt="Avatar">
                    </div>
                    <div class="user-info">
                        <div class="user-name">${data.reporterName}</div>
                        <div class="user-email">${
                          data.reporterEmail || ""
                        }</div>
                    </div>
                </div>`;
    }
    document.getElementById("modalReporterInfo").innerHTML = reporterHtml;

    // Reported User Info
    let reportedUserHtml = "N/A";
    if (data.reportedUserName) {
      reportedUserHtml = `
                <div class="user-cell">
                    <div class="user-avatar">
                        <img src="${data.reportedUserAvatar}" alt="Avatar">
                    </div>
                    <div class="user-info">
                        <div class="user-name">${data.reportedUserName}</div>
                        <div class="user-email">${
                          data.reportedUserEmail || ""
                        }</div>
                    </div>
                </div>`;
    }
    document.getElementById("modalReportedUserInfo").innerHTML =
      reportedUserHtml;

    document.getElementById("modalTypeOfReport").textContent =
      data.typeOfReport || "N/A";
    // Cập nhật class cho badge nếu cần
    // document.getElementById('modalTypeOfReport').className = `badge ${getReportTypeClass(data.typeOfReport)}`;

    document.getElementById("modalTargetType").textContent =
      data.targetType || "N/A";
    document.getElementById("modalTargetLink").href = data.targetLink || "#";
    if (data.targetLink && data.targetLink !== "#") {
      document.getElementById("modalTargetLink").style.display = "inline";
      document
        .getElementById("modalTargetLink")
        .setAttribute("target", "_blank"); // Mở link ở tab mới
    } else {
      document.getElementById("modalTargetLink").style.display = "none";
    }

    document.getElementById("modalReportedAt").textContent = data.reportedAt;
    document.getElementById("modalReason").textContent = data.reason;

    const modalStatusSelect = document.getElementById("modalNewStatus");
    modalStatusSelect.value = data.status;

    const targetContentSection = document.getElementById(
      "modalTargetContentSection"
    );
    const targetContentDiv = document.getElementById("modalTargetContent");
    if (
      data.targetContent &&
      (data.targetContent.content || data.targetContent.title)
    ) {
      let contentHtml = "";
      if (data.targetContent.type === "Post" && data.targetContent.title) {
        contentHtml += `<strong>Tiêu đề:</strong> ${data.targetContent.title}<br>`;
      }
      if (data.targetContent.content) {
        contentHtml += `<strong>Nội dung:</strong> ${data.targetContent.content}`;
      }
      targetContentDiv.innerHTML = contentHtml;
      targetContentSection.style.display = "block";
    } else if (data.targetContent && data.targetContent.type === "User") {
      targetContentDiv.innerHTML =
        "Đối tượng báo cáo là một người dùng. Xem chi tiết người dùng qua link ở trên.";
      targetContentSection.style.display = "block";
    } else {
      targetContentSection.style.display = "none";
      targetContentDiv.innerHTML = "";
    }

    document.getElementById("modalAdminNotes").value = data.adminNotes || "";

    const actionDeleteContent = document.getElementById(
      "actionDeleteContentOption"
    );
    const actionWarnUser = document.getElementById("actionWarnUserOption");
    const actionBanUser = document.getElementById("actionBanUserOption");

    // Chỉ hiển thị "Xóa nội dung" nếu target là Post hoặc Comment
    actionDeleteContent.style.display =
      data.targetType === "Post" ||
      data.targetType === "Comment" ||
      data.targetType === "Review"
        ? "block"
        : "none";
    // Chỉ hiển thị "Cảnh báo/Cấm người dùng" nếu có reportedUserId
    const hasReportedUser = !!data.reportedUserId;
    actionWarnUser.style.display = hasReportedUser ? "block" : "none";
    actionBanUser.style.display = hasReportedUser ? "block" : "none";

    // Reset selected action về "Chỉ cập nhật trạng thái"
    document.getElementById("action-ignore-report").checked = true;
    // Tự động check "Xóa nội dung" nếu target là Post/Comment và không có reported user
    // (gợi ý cho admin, admin vẫn có thể đổi)
    // if ((data.targetType === "Post" || data.targetType === "Comment") && !hasReportedUser) {
    //     const radioDelete = document.getElementById("action-delete-content");
    //     if(radioDelete) radioDelete.checked = true;
    // }
  }

  // --- Xử lý submit form trong modal ---
  if (processReportForm) {
    processReportForm.addEventListener("submit", function (e) {
      e.preventDefault(); // Ngăn submit form mặc định

      const formData = new FormData(this);
      // TODO: Hiển thị loading/spinner cho nút submit

      fetch(this.action, {
        method: "POST",
        body: formData,
        // headers: { // FormData tự set Content-Type, không cần 'Content-Type': 'application/x-www-form-urlencoded'
        //     // AntiForgeryToken thường được gửi qua form data
        // }
      })
        .then((response) => {
          // Nếu controller redirect, response.redirected sẽ là true
          // và response.url sẽ là URL mới.
          // Trong trường hợp này, ta muốn reload để TempData hiển thị.
          if (response.ok || response.redirected) {
            closeModal();
            location.reload(); // Tải lại trang để hiển thị TempData và cập nhật bảng
          } else {
            // Xử lý lỗi từ server (ví dụ: validation errors nếu controller trả về JSON)
            // response.text().then(text => alert("Lỗi: " + text));
            alert(
              "Xảy ra lỗi khi xử lý báo cáo. Vui lòng kiểm tra lại thông tin hoặc thử lại sau."
            );
          }
        })
        .catch((error) => {
          console.error("Error submitting report processing:", error);
          alert("Xảy ra lỗi kết nối khi xử lý báo cáo. Vui lòng thử lại.");
        })
        .finally(() => {
          // TODO: Ẩn loading/spinner
        });
    });
  }
});

function onStatusFilterChange(selectElement) {
  const selectedValue = selectElement.value;
  document.getElementById("hiddenStatusFilter").value = selectedValue;
  document.getElementById("filterForm").submit();
}
