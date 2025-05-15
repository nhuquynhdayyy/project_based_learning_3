document.addEventListener("DOMContentLoaded", () => {
  // Mobile sidebar toggle (giữ nguyên)
  const toggleSidebarBtn = document.querySelector(".toggle-sidebar");
  if (toggleSidebarBtn) {
    toggleSidebarBtn.addEventListener("click", () => {
      document.querySelector(".sidebar").classList.toggle("open");
    });
  }

  // Chart instances (khai báo ở scope rộng hơn để có thể truy cập và cập nhật)
  window.charts = {}; // To store chart instances

  // Fetch and initialize data
  fetchAndLoadData();

  // Time range filter
  const timeRangeSelect = document.getElementById("time-range");
  if (timeRangeSelect) {
    timeRangeSelect.addEventListener("change", function () {
      fetchAndLoadData();
    });
  }

  // Traffic interval filter
  const trafficIntervalSelect = document.getElementById("traffic-interval");
  if (trafficIntervalSelect) {
    trafficIntervalSelect.addEventListener("change", function () {
      fetchAndLoadData(); // Gọi lại để lấy dữ liệu traffic chart mới
    });
  }

  // Export report button (giữ nguyên logic alert, hoặc triển khai thực tế)
  const exportReportBtn = document.getElementById("exportReportBtn");
  if (exportReportBtn) {
    exportReportBtn.addEventListener("click", () => {
      alert("Báo cáo đang được xuất. Vui lòng đợi trong giây lát...");
      // In a real application, this would generate and download a report
      // You might make an API call here to a backend endpoint that generates the report
    });
  }
});

async function fetchAndLoadData() {
  const timeRange = document.getElementById("time-range").value;
  const trafficInterval = document.getElementById("traffic-interval").value;

  // Add a loading indicator if desired
  showLoading(true);

  try {
    // API_URL_GET_STATISTICS được định nghĩa trong file .cshtml
    const response = await fetch(
      `${API_URL_GET_STATISTICS}?timeRange=${timeRange}&trafficInterval=${trafficInterval}`
    );
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    const data = await response.json();

    updateStatsCards(data.newUsersCard, data.interactionRateCard);
    initializeOrUpdateAllCharts(data);
  } catch (error) {
    console.error("Could not fetch statistics data:", error);
    // Display an error message to the user
  } finally {
    showLoading(false); // Hide loading indicator
  }
}

function showLoading(isLoading) {
  // Implement a simple loading indicator, e.g., by showing/hiding an overlay
  const loadingOverlay = document.getElementById("loadingOverlay"); // Bạn cần tạo element này trong HTML
  if (loadingOverlay) {
    loadingOverlay.style.display = isLoading ? "flex" : "none";
  } else if (isLoading) {
    console.log("Loading data..."); // Fallback
  }
}

function updateStatsCards(newUsersData, interactionRateData) {
  // New Users Card
  const newUsersCardEl = document.getElementById("newUsersCard");
  if (newUsersCardEl && newUsersData) {
    newUsersCardEl.querySelector(".stats-card-label").textContent =
      newUsersData.label;
    newUsersCardEl.querySelector(".stats-card-value").textContent =
      newUsersData.value;
    const trendEl = newUsersCardEl.querySelector(".stats-trend");
    trendEl.textContent = newUsersData.trendPercentage;
    trendEl.className = `stats-trend ${
      newUsersData.isPositiveTrend ? "positive" : "negative"
    }`;
    newUsersCardEl.querySelector(".comparison-text").textContent =
      newUsersData.comparisonText;
    const iconDiv = newUsersCardEl.querySelector(".stats-card-icon");
    iconDiv.className = `stats-card-icon ${newUsersData.iconColorClass}`; // Cập nhật màu icon
    iconDiv.querySelector("i").className = newUsersData.iconClass; // Cập nhật class icon
  }

  // Interaction Rate Card
  const interactionRateCardEl = document.getElementById("interactionRateCard");
  if (interactionRateCardEl && interactionRateData) {
    interactionRateCardEl.querySelector(".stats-card-label").textContent =
      interactionRateData.label;
    interactionRateCardEl.querySelector(".stats-card-value").textContent =
      interactionRateData.value;
    const trendEl = interactionRateCardEl.querySelector(".stats-trend");
    trendEl.textContent = interactionRateData.trendPercentage;
    trendEl.className = `stats-trend ${
      interactionRateData.isPositiveTrend ? "positive" : "negative"
    }`;
    interactionRateCardEl.querySelector(".comparison-text").textContent =
      interactionRateData.comparisonText;
    const iconDiv = interactionRateCardEl.querySelector(".stats-card-icon");
    iconDiv.className = `stats-card-icon ${interactionRateData.iconColorClass}`;
    iconDiv.querySelector("i").className = interactionRateData.iconClass;
  }
}

function initializeOrUpdateAllCharts(apiData) {
  // Traffic Chart
  if (apiData.trafficChart) {
    document.getElementById("trafficChartTitle").textContent =
      apiData.trafficChart.title || "Lượng truy cập theo thời gian";
    initializeOrUpdateChart("trafficChart", "line", apiData.trafficChart, {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: { display: true, position: "bottom" },
        tooltip: { mode: "index", intersect: false },
      },
      scales: {
        x: { grid: { display: true, color: "#f0f0f0" } },
        y: {
          beginAtZero: true,
          position: "left",
          grid: { color: "#f0f0f0" },
          ticks: {
            callback: (v) => (v >= 1000 ? (v / 1000).toFixed(1) + "k" : v),
          },
        },
        // y1: { position: "right", grid: { display: false }, beginAtZero: true } // Bỏ y1 nếu chỉ có 1 trục Y chính
      },
    });
  }

  // Post Distribution Chart (Pie Chart)
  if (apiData.postDistributionChart) {
    document.getElementById("postDistributionChartTitle").textContent =
      apiData.postDistributionChart.title || "Phân bố bài viết theo loại";
    initializeOrUpdateChart(
      "sourceChart",
      "pie",
      apiData.postDistributionChart,
      {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { position: "bottom" },
          tooltip: {
            callbacks: { label: (c) => `${c.label || ""}: ${c.raw || 0}%` },
          },
        },
      }
    );
  }

  // User Interactions Chart (Line Chart)
  if (apiData.userInteractionsChart) {
    document.getElementById("userInteractionsChartTitle").textContent =
      apiData.userInteractionsChart.title ||
      "Tương tác người dùng theo thời gian";
    initializeOrUpdateChart(
      "interactionsChart",
      "line",
      apiData.userInteractionsChart,
      {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { display: true, position: "bottom" },
          // title: { display: true, text: apiData.userInteractionsChart.title, font: { size: 16, weight: 'bold' } }, // Tiêu đề đã set ở h2
          tooltip: { mode: "index", intersect: false },
        },
        scales: {
          x: { grid: { display: true, color: "#f0f0f0" } },
          y: {
            beginAtZero: true,
            grid: { color: "#f0f0f0" },
            ticks: {
              callback: (v) => (v >= 1000 ? (v / 1000).toFixed(1) + "k" : v),
            },
          },
        },
      }
    );
  }

  // Top 5 Posts Chart (Horizontal Bar Chart)
  if (apiData.topPostsChart) {
    document.getElementById("topPostsChartTitle").textContent =
      apiData.topPostsChart.title || "Top 5 bài viết được xem nhiều nhất";
    initializeOrUpdateChart("deviceChart", "bar", apiData.topPostsChart, {
      indexAxis: "y",
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: { display: false },
        // title: { display: true, text: apiData.topPostsChart.title, font: { size: 16, weight: 'bold' } },
        tooltip: {
          callbacks: {
            label: (c) => `Lượt xem: ${(c.raw || 0).toLocaleString()}`,
          },
        },
      },
      scales: {
        x: {
          grid: { display: true, color: "#f0f0f0", drawBorder: false },
          ticks: {
            stepSize: 3500,
            callback: (v) => (v >= 1000 ? v / 1000 + "k" : v),
          },
          border: { display: false },
        },
        y: { grid: { display: false }, border: { display: false } },
      },
      layout: { padding: { top: 10, right: 20, bottom: 10, left: 10 } },
    });
  }

  // Location Distribution Chart (Pie Chart)
  if (apiData.locationDistributionChart) {
    document.getElementById("locationDistributionChartTitle").textContent =
      apiData.locationDistributionChart.title ||
      "Phân bố bài viết theo địa điểm";
    initializeOrUpdateChart(
      "ageChart",
      "pie",
      apiData.locationDistributionChart,
      {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            position: "right",
            align: "center",
            labels: { usePointStyle: true, padding: 15, font: { size: 12 } },
          },
          // title: { display: true, text: apiData.locationDistributionChart.title, font: { size: 16, weight: 'bold' } },
          tooltip: {
            callbacks: { label: (c) => `${c.label || ""}: ${c.raw || 0}` },
          },
        },
      }
    );
  }
}

function initializeOrUpdateChart(canvasId, chartType, chartData, chartOptions) {
  const ctx = document.getElementById(canvasId)?.getContext("2d");
  if (!ctx) {
    console.error(`Canvas with id ${canvasId} not found.`);
    return;
  }

  if (window.charts[canvasId]) {
    // Update existing chart
    window.charts[canvasId].data.labels = chartData.labels;
    window.charts[canvasId].data.datasets = chartData.datasets; // Đây là mảng các dataset objects
    window.charts[canvasId].options = {
      ...window.charts[canvasId].options,
      ...chartOptions,
    }; // Merge options if needed, or just re-assign
    window.charts[canvasId].update();
  } else {
    // Create new chart
    window.charts[canvasId] = new Chart(ctx, {
      type: chartType,
      data: {
        labels: chartData.labels,
        datasets: chartData.datasets, // Đây là mảng các dataset objects
      },
      options: chartOptions,
    });
  }
}

// Hàm updateTrafficChart cũ không còn cần thiết nếu fetchAndLoadData xử lý tất cả
// Tuy nhiên, nếu bạn muốn giữ nó để chỉ cập nhật traffic chart thì có thể điều chỉnh
// function updateTrafficChartWithDynamicData(interval, data) {
//   if (window.charts['trafficChart']) {
//     window.charts['trafficChart'].data.labels = data.labels;
//     window.charts['trafficChart'].data.datasets.forEach((dataset, index) => {
//       if (data.datasets[index]) {
//         dataset.data = data.datasets[index].data;
//       }
//     });
//     window.charts['trafficChart'].update();
//   }
// }
