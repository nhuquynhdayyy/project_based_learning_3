document.addEventListener("DOMContentLoaded", function () {
  // Xử lý thay đổi filter thời gian
  const timeRangeSelect = document.getElementById("time-range");
  if (timeRangeSelect) {
    // Đặt giá trị đã chọn cho select (nếu được truyền từ server qua biến selectedTimeRange)
    if (typeof selectedTimeRange !== "undefined" && selectedTimeRange) {
      timeRangeSelect.value = selectedTimeRange;
    }

    timeRangeSelect.addEventListener("change", function () {
      const selectedValue = this.value;
      // Chuyển hướng trang với query string mới
      window.location.href =
        window.location.pathname + "?timeRange=" + selectedValue;
    });
  }

  // Biểu đồ số bài viết
  const postsCtx = document.getElementById("postsChart");
  if (
    postsCtx &&
    typeof postChartLabels !== "undefined" &&
    typeof postChartData !== "undefined"
  ) {
    new Chart(postsCtx, {
      type: "line", // Hoặc 'bar'
      data: {
        labels: postChartLabels, // Sử dụng biến từ View
        datasets: [
          {
            label: "Số bài viết",
            data: postChartData, // Sử dụng biến từ View
            borderColor: "rgb(75, 192, 192)",
            backgroundColor: "rgba(75, 192, 192, 0.2)",
            tension: 0.1,
            fill: true,
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        scales: {
          y: {
            beginAtZero: true,
            ticks: {
              stepSize: 1, // Đảm bảo trục y chỉ hiển thị số nguyên nếu số lượng nhỏ
            },
          },
        },
      },
    });
  }

  // Biểu đồ tỷ lệ phân bố
  const distributionCtx = document.getElementById("distributionChart");
  if (
    distributionCtx &&
    typeof distributionChartLabels !== "undefined" &&
    typeof distributionChartData !== "undefined"
  ) {
    new Chart(distributionCtx, {
      type: "doughnut", // Hoặc 'pie'
      data: {
        labels: distributionChartLabels, // Sử dụng biến từ View
        datasets: [
          {
            label: "Phân bố bài viết",
            data: distributionChartData, // Sử dụng biến từ View
            backgroundColor: [
              // Thêm màu cho từng phần
              "rgb(255, 99, 132)",
              "rgb(54, 162, 235)",
              "rgb(255, 205, 86)",
              "rgb(75, 192, 192)",
              "rgb(153, 102, 255)",
              "rgb(255, 159, 64)",
            ],
            hoverOffset: 4,
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
      },
    });
  }

  // Toggle sidebar cho mobile (giữ lại nếu bạn có logic này)
  const sidebar = document.querySelector(".sidebar");
  const toggleButton = document.querySelector(".toggle-sidebar");

  if (sidebar && toggleButton) {
    toggleButton.addEventListener("click", () => {
      sidebar.classList.toggle("active");
    });
  }
});
