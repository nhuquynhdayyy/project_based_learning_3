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

  // // Biểu đồ tỷ lệ phân bố
  // const distributionCtx = document.getElementById("distributionChart");
  // if (
  //   distributionCtx &&
  //   typeof distributionChartLabels !== "undefined" &&
  //   typeof distributionChartData !== "undefined"
  // ) {
  //   new Chart(distributionCtx, {
  //     type: "doughnut", // Hoặc 'pie'
  //     data: {
  //       labels: distributionChartLabels, // Sử dụng biến từ View
  //       datasets: [
  //         {
  //           label: "Tỷ lệ phân bố",
  //           data: distributionChartData, // Sử dụng biến từ View
  //           backgroundColor: [
  //             // Thêm màu cho từng phần
  //             "rgb(255, 99, 132)",
  //             "rgb(54, 162, 235)",
  //             "rgb(255, 205, 86)",
  //             "rgb(75, 192, 192)",
  //             "rgb(153, 102, 255)",
  //             "rgb(255, 159, 64)",
  //           ],
  //           hoverOffset: 4,
  //         },
  //       ],
  //     },
  //     options: {
  //       responsive: true,
  //       maintainAspectRatio: false,
  //     },
  //   });
  // }
  <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>;
  <script src="https://cdn.jsdelivr.net/npm/chartjs-plugin-datalabels@2"></script>;

  // Biểu đồ tỷ lệ phân bố
  const distributionCtx = document.getElementById("distributionChart");

  // Đăng ký plugin nếu dùng Chart.js v3+ trở lên
  Chart.register(ChartDataLabels);

  if (
    distributionCtx &&
    typeof distributionChartLabels !== "undefined" &&
    typeof distributionChartData !== "undefined"
  ) {
    new Chart(distributionCtx, {
      type: "doughnut", // hoặc 'pie'
      data: {
        labels: distributionChartLabels,
        datasets: [
          {
            label: "Phân bố bài viết",
            data: distributionChartData,
            backgroundColor: [
              "rgb(255, 99, 132)", // Cẩm nang
              "rgb(54, 162, 235)", // Trải nghiệm
              "rgb(255, 205, 86)", // Địa điểm
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
        plugins: {
          tooltip: {
            callbacks: {
              label: function (context) {
                let datasetLabel = context.dataset.label || "";
                let value = context.raw;
                const numericValue = parseFloat(value);

                const datasetData =
                  context.chart.data.datasets[context.datasetIndex].data;
                const total = datasetData.reduce((sum, currentValue) => {
                  const val = parseFloat(currentValue);
                  return sum + (isNaN(val) ? 0 : val);
                }, 0);

                let percentageText = "0%";
                if (total > 0 && !isNaN(numericValue)) {
                  percentageText =
                    ((numericValue / total) * 100).toFixed(1) + "%";
                }

                return datasetLabel + ": " + percentageText;
              },
            },
          },
          datalabels: {
            color: "#fff",
            font: {
              weight: "bold",
            },
            formatter: (value, ctx) => {
              let sum = 0;
              let dataArr = ctx.chart.data.datasets[0].data;
              dataArr.forEach((data) => {
                const numData = parseFloat(data);
                if (!isNaN(numData)) sum += numData;
              });
              if (sum === 0) return "";
              const numericItemValue = parseFloat(value);
              if (isNaN(numericItemValue) || numericItemValue <= 0) return "";
              let percentage =
                ((numericItemValue * 100) / sum).toFixed(1) + "%";
              return percentage;
            },
          },
        },
      },
      plugins: [ChartDataLabels], // <<< Đừng quên dòng này để plugin hoạt động
    });
  }

  // Biểu đồ tỷ lệ phân bố
  // const distributionCtx = document.getElementById("distributionChart");
  // if (
  //   distributionCtx &&
  //   typeof distributionChartLabels !== "undefined" &&
  //   typeof distributionChartData !== "undefined"
  // ) {
  //   new Chart(distributionCtx, {
  //     type: "doughnut", // Hoặc 'pie'
  //     data: {
  //       labels: distributionChartLabels, // Sử dụng biến từ View
  //       datasets: [
  //         {
  //           // label: "Tỷ lệ phân bố", // Bạn có thể giữ nguyên hoặc đổi
  //           label: "Phân bố bài viết", // <<< THAY ĐỔI Ở ĐÂY để khớp với yêu cầu "Phân bố bài viết: XX%"
  //           data: distributionChartData, // Sử dụng biến từ View (đảm bảo đây là mảng số)
  //           backgroundColor: [
  //             "rgb(255, 99, 132)", // Cẩm nang
  //             "rgb(54, 162, 235)", // Trải nghiệm
  //             "rgb(255, 205, 86)", // Bài viết về Địa điểm
  //             "rgb(75, 192, 192)",
  //             "rgb(153, 102, 255)",
  //             "rgb(255, 159, 64)",
  //           ],
  //           hoverOffset: 4,
  //         },
  //       ],
  //     },
  //     options: {
  //       responsive: true,
  //       maintainAspectRatio: false,
  //       plugins: {
  //         // <<< BẮT ĐẦU THÊM CẤU HÌNH PLUGIN
  //         tooltip: {
  //           callbacks: {
  //             // title: function(tooltipItems) {
  //             //     // Mặc định title của tooltip sẽ là label của slice (ví dụ: "Bài viết về Địa điểm")
  //             //     // Bạn có thể tùy chỉnh nếu muốn
  //             //     if (tooltipItems.length > 0) {
  //             //         return tooltipItems[0].label;
  //             //     }
  //             //     return '';
  //             // },
  //             label: function (context) {
  //               // context ở đây là một TooltipItem
  //               let datasetLabel = context.dataset.label || ""; // Lấy "Phân bố bài viết" từ dataset
  //               let value = context.raw; // Giá trị số lượng thô của slice này, ví dụ 33

  //               // Chuyển đổi value sang số, phòng trường hợp nó là chuỗi
  //               const numericValue = parseFloat(value);

  //               // Lấy toàn bộ dữ liệu của dataset hiện tại để tính tổng
  //               const datasetData =
  //                 context.chart.data.datasets[context.datasetIndex].data;
  //               const total = datasetData.reduce((sum, currentValue) => {
  //                 // Chuyển đổi currentValue sang số và cộng vào sum
  //                 const val = parseFloat(currentValue);
  //                 return sum + (isNaN(val) ? 0 : val); // Nếu không phải số, coi như 0
  //               }, 0);

  //               let percentageText = "0%";
  //               // Tính toán phần trăm nếu total > 0 và numericValue là một số hợp lệ
  //               if (total > 0 && !isNaN(numericValue)) {
  //                 percentageText =
  //                   ((numericValue / total) * 100).toFixed(1) + "%";
  //               }

  //               // Ghép dataset label (nếu có) với percentageText
  //               if (datasetLabel) {
  //                 return datasetLabel + ": " + percentageText; // Ví dụ: "Phân bố bài viết: 33.3%"
  //               }
  //               return percentageText; // Fallback nếu không có dataset.label
  //             },
  //           },
  //         },
  //         // Nếu bạn cũng muốn hiển thị phần trăm trực tiếp trên các slice của biểu đồ (không chỉ trong tooltip)
  //         // thì bạn cần plugin chartjs-plugin-datalabels và cấu hình nó ở đây.
  //         // Ví dụ (yêu cầu bạn đã import plugin):
  //         /*
  //       datalabels: {
  //           formatter: (value, ctx) => {
  //               let sum = 0;
  //               let dataArr = ctx.chart.data.datasets[0].data;
  //               dataArr.map(data => {
  //                   const numData = parseFloat(data);
  //                   if (!isNaN(numData)) sum += numData;
  //               });
  //               if (sum === 0) return '';
  //               const numericItemValue = parseFloat(value);
  //               if (isNaN(numericItemValue) || numericItemValue <= 0) return '';
  //               let percentage = (numericItemValue * 100 / sum).toFixed(1) + "%";
  //               return percentage;
  //           },
  //           color: '#fff', // Màu chữ cho datalabels
  //           // ... các tùy chọn khác cho datalabels
  //       }
  //       */
  //       }, // <<< KẾT THÚC CẤU HÌNH PLUGIN
  //     },
  //   });
  // }

  // Toggle sidebar cho mobile (giữ lại nếu bạn có logic này)
  const sidebar = document.querySelector(".sidebar");
  const toggleButton = document.querySelector(".toggle-sidebar");

  if (sidebar && toggleButton) {
    toggleButton.addEventListener("click", () => {
      sidebar.classList.toggle("active");
    });
  }
});
