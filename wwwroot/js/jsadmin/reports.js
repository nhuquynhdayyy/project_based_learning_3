// document.addEventListener("DOMContentLoaded", () => {
//   // Mobile sidebar toggle
//   const toggleSidebarBtn = document.querySelector(".toggle-sidebar")
//   if (toggleSidebarBtn) {
//     toggleSidebarBtn.addEventListener("click", () => {
//       document.querySelector(".sidebar").classList.toggle("open")
//     })
//   }

//   // Report status filter
//   const reportStatusSelect = document.getElementById("report-status")
//   if (reportStatusSelect) {
//     reportStatusSelect.addEventListener("change", function () {
//       // In a real application, this would filter reports based on status
//       console.log("Status filter changed to:", this.value)
//     })
//   }

//   // View Report Buttons
//   const viewButtons = document.querySelectorAll(".view-btn")
//   const reportModal = document.getElementById("reportModal")
//   if (viewButtons && reportModal) {
//     viewButtons.forEach((button) => {
//       button.addEventListener("click", () => {
//         reportModal.classList.add("show")
//       })
//     })
//   }

//   // Cancel Action Button
//   const cancelActionBtn = document.getElementById("cancelAction")
//   if (cancelActionBtn) {
//     cancelActionBtn.addEventListener("click", () => {
//       reportModal.classList.remove("show")
//     })
//   }

//   // Submit Action Button
//   const submitActionBtn = document.getElementById("submitAction")
//   if (submitActionBtn) {
//     submitActionBtn.addEventListener("click", () => {
//       const selectedAction = document.querySelector('input[name="action"]:checked')
//       if (!selectedAction) {
//         alert("Vui lòng chọn hành động xử lý!")
//         return
//       }

//       const actionValue = selectedAction.value
//       const actionNote = document.getElementById("action-note-text").value
//       const reportStatus = document.getElementById("report-status-select").value

//       // In a real application, this would send the action to the server
//       console.log("Action:", actionValue)
//       console.log("Note:", actionNote)
//       console.log("Status:", reportStatus)

//       alert("Đã xử lý báo cáo thành công!")
//       reportModal.classList.remove("show")
//     })
//   }

//   // Resolve Report Buttons
//   const resolveButtons = document.querySelectorAll(".resolve-btn")
//   if (resolveButtons) {
//     resolveButtons.forEach((button) => {
//       button.addEventListener("click", function () {
//         const row = this.closest("tr")
//         const statusCell = row.querySelector("td:nth-child(7)")

//         // Update status
//         statusCell.innerHTML = '<span class="badge green">Đã xử lý</span>'

//         // Remove resolve and dismiss buttons
//         this.remove()
//         const dismissBtn = row.querySelector(".dismiss-btn")
//         if (dismissBtn) dismissBtn.remove()

//         alert("Báo cáo đã được đánh dấu là đã xử lý!")
//       })
//     })
//   }

//   // Dismiss Report Buttons
//   const dismissButtons = document.querySelectorAll(".dismiss-btn")
//   if (dismissButtons) {
//     dismissButtons.forEach((button) => {
//       button.addEventListener("click", function () {
//         const row = this.closest("tr")
//         const statusCell = row.querySelector("td:nth-child(7)")

//         // Update status
//         statusCell.innerHTML = '<span class="badge red">Đã bỏ qua</span>'

//         // Remove resolve and dismiss buttons
//         this.remove()
//         const resolveBtn = row.querySelector(".resolve-btn")
//         if (resolveBtn) resolveBtn.remove()

//         alert("Báo cáo đã được đánh dấu là đã bỏ qua!")
//       })
//     })
//   }

//   // Report Status Change in Modal
//   const reportStatusSelectModal = document.getElementById("report-status-select")
//   if (reportStatusSelectModal) {
//     reportStatusSelectModal.addEventListener("change", function () {
//       // In a real application, this would update the report status
//       console.log("Status changed to:", this.value)
//     })
//   }

//   // Close modals when clicking outside
//   window.addEventListener("click", (event) => {
//     if (event.target === reportModal) {
//       reportModal.classList.remove("show")
//     }
//   })

//   // Close Modal Buttons
//   const modalCloseButtons = document.querySelectorAll(".modal-close")
//   if (modalCloseButtons) {
//     modalCloseButtons.forEach((button) => {
//       button.addEventListener("click", () => {
//         document.querySelectorAll(".modal").forEach((modal) => {
//           modal.classList.remove("show")
//         })
//       })
//     })
//   }
// })
document.addEventListener("DOMContentLoaded", function () {
  const reportModal = document.getElementById("reportModal");
  const closeModalButtons = document.querySelectorAll(
    ".modal-close, .modal-close-btn"
  );
  const filterForm = document.getElementById("filterForm");
  const mainStatusFilter = document.getElementById("report-status-filter-main");
  const hiddenStatusFilterInput = document.getElementById("hiddenStatusFilter");
  const pageSizeSelect = document.getElementById("pageSizeSelect");

  // --- Xử lý Filter ---
  if (mainStatusFilter && filterForm && hiddenStatusFilterInput) {
    mainStatusFilter.addEventListener("change", function () {
      hiddenStatusFilterInput.value = this.value;
      filterForm.requestSubmit(); // Submit form để lọc
    });
  }

  if (pageSizeSelect && filterForm) {
    pageSizeSelect.addEventListener("change", function () {
      const pageSizeInput = filterForm.querySelector('input[name="pageSize"]');
      if (pageSizeInput) {
        pageSizeInput.value = this.value;
      } else {
        // Nếu chưa có thì tạo mới
        let newInput = document.createElement("input");
        newInput.type = "hidden";
        newInput.name = "pageSize";
        newInput.value = this.value;
        filterForm.appendChild(newInput);
      }
      // Đảm bảo pageNumber reset về 1 khi đổi pageSize
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

  // Mở modal khi click nút "Xem" hoặc "Xử lý"
  document.querySelectorAll(".view-btn, .process-btn").forEach((button) => {
    button.addEventListener("click", function () {
      const reportId = this.dataset.reportId;
      fetchReportDetails(reportId);
    });
  });

  function fetchReportDetails(reportId) {
    // Hiển thị loading indicator nếu có
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
      data.typeOfReport;
    // Bạn có thể thêm class màu cho badge type ở đây nếu muốn
    // document.getElementById('modalTypeOfReport').className = `badge ${getReportTypeClass(data.typeOfReport)}`;

    document.getElementById("modalTargetType").textContent = data.targetType;
    document.getElementById("modalTargetLink").href = data.targetLink || "#";
    if (data.targetLink && data.targetLink !== "#") {
      document.getElementById("modalTargetLink").style.display = "inline";
    } else {
      document.getElementById("modalTargetLink").style.display = "none";
    }

    document.getElementById("modalReportedAt").textContent = data.reportedAt;
    document.getElementById("modalReason").textContent = data.reason;

    // Trạng thái xử lý
    const modalStatusSelect = document.getElementById("modalNewStatus");
    modalStatusSelect.value = data.status; // "Pending", "Resolved", "Dismissed"

    // Nội dung bị báo cáo
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
    } else {
      targetContentSection.style.display = "none";
      targetContentDiv.innerHTML = "";
    }

    // Ghi chú admin
    document.getElementById("modalAdminNotes").value = data.adminNotes || "";

    // Ẩn/Hiện các action tùy theo đối tượng và người bị báo cáo
    const actionDeleteContent = document.getElementById(
      "actionDeleteContentOption"
    );
    const actionWarnUser = document.getElementById("actionWarnUserOption");
    const actionBanUser = document.getElementById("actionBanUserOption");

    actionDeleteContent.style.display =
      data.targetType === "Post" || data.targetType === "Comment"
        ? "block"
        : "none";
    actionWarnUser.style.display = data.reportedUserId ? "block" : "none";
    actionBanUser.style.display = data.reportedUserId ? "block" : "none";

    // Reset selected action
    document.getElementById("action-ignore-report").checked = true;
  }

  // (Tùy chọn) Helper JS để lấy class màu, nếu bạn không muốn dùng Razor @functions
  // function getReportTypeClass(typeString) { ... }
});

$(document).on("click", ".process-btn", function () {
  const reportId = $(this).data("report-id");

  $.get(`/Admin/GetReportDetails/${reportId}`, function (data) {
    // Điền dữ liệu vào modal
    $("#modalReportId").text(data.reportId);
    $("#modalHiddenReportId").val(data.reportId);
    $("#modalHiddenReportedUserId").val(data.reportedUserId);

    $("#modalReporterInfo").html(
      `${data.reporterName} (${data.reporterEmail}) <img src="${data.reporterAvatar}" width="30" />`
    );
    $("#modalTypeOfReport").text(data.typeOfReport);
    $("#modalTargetType").text(data.targetType);
    $("#modalTargetLink").attr("href", data.targetLink);
    $("#modalReportedUserInfo").html(
      `${data.reportedUserName} (${data.reportedUserEmail}) <img src="${data.reportedUserAvatar}" width="30" />`
    );
    $("#modalReportedAt").text(data.reportedAt);
    $("#modalNewStatus").val(data.status);
    $("#modalReason").text(data.reason);
    $("#modalAdminNotes").val(data.adminNotes || "");

    if (data.targetContent && data.targetContent.content) {
      $("#modalTargetContent").html(data.targetContent.content);
      $("#modalTargetContentSection").show();
    } else {
      $("#modalTargetContentSection").hide();
    }

    // Hiển thị action phù hợp
    $("#actionDeleteContentOption").toggle(
      data.targetContent?.type === "Post" ||
        data.targetContent?.type === "Comment"
    );
    $("#actionWarnUserOption, #actionBanUserOption").toggle(
      !!data.reportedUserId
    );

    // Hiện modal
    $("#reportModal").show();
  });
});

$(".modal-close, .modal-close-btn").on("click", function () {
  $("#reportModal").hide();
});

$("#modalAdminNotes").val(data.adminNotes || "");

if (data.targetContent) {
  const contentSummary = data.targetContent.title
    ? `<strong>${data.targetContent.title}</strong><br>${data.targetContent.content}`
    : data.targetContent.content;

  $("#modalTargetContent").html(contentSummary);
  $("#modalTargetContentSection").show();
} else {
  $("#modalTargetContentSection").hide();
}

$("#processReportForm").submit(function (e) {
  e.preventDefault(); // Ngăn submit mặc định
  const form = $(this);
  $.ajax({
    type: "POST",
    url: form.attr("action"),
    data: form.serialize(),
    success: function () {
      alert("Đã xử lý báo cáo.");
      $("#reportModal").hide();
      // Có thể reload bảng báo cáo hoặc cập nhật trạng thái từng dòng nếu dùng DataTables, etc.
    },
    error: function () {
      alert("Xử lý thất bại.");
    },
  });
});
