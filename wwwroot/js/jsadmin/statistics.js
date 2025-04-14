document.addEventListener("DOMContentLoaded", () => {
  // Mobile sidebar toggle
  const toggleSidebarBtn = document.querySelector(".toggle-sidebar")
  if (toggleSidebarBtn) {
    toggleSidebarBtn.addEventListener("click", () => {
      document.querySelector(".sidebar").classList.toggle("open")
    })
  }

  // Time range filter
  const timeRangeSelect = document.getElementById("time-range")
  if (timeRangeSelect) {
    timeRangeSelect.addEventListener("change", function () {
      // In a real application, this would fetch new data based on the selected time range
      console.log("Time range changed to:", this.value)
      // For demo purposes, we'll just reinitialize the charts
      initializeCharts()
    })
  }

  // Traffic interval filter
  const trafficIntervalSelect = document.getElementById("traffic-interval")
  if (trafficIntervalSelect) {
    trafficIntervalSelect.addEventListener("change", function () {
      // In a real application, this would update the traffic chart based on the selected interval
      console.log("Traffic interval changed to:", this.value)
      // For demo purposes, we'll just update the traffic chart
      updateTrafficChart(this.value)
    })
  }

  // Export report button
  const exportReportBtn = document.querySelector(".btn-secondary")
  if (exportReportBtn) {
    exportReportBtn.addEventListener("click", () => {
      alert("Báo cáo đang được xuất. Vui lòng đợi trong giây lát...")
      // In a real application, this would generate and download a report
    })
  }

  // Initialize charts
  initializeCharts()
})

function initializeCharts() {
  // Traffic Chart (Line Chart)
  initializeTrafficChart()
  
  // Source Chart (Pie Chart) - Updated to show post types distribution
  const sourceChartCtx = document.getElementById("sourceChart").getContext("2d")
  const sourceChart = new Chart(sourceChartCtx, {
    type: "pie",
    data: {
      labels: ["Địa điểm", "Cẩm nang", "Trải nghiệm"],
      datasets: [
        {
          data: [38, 34, 28],
          backgroundColor: ["#3b82f6", "#10b981", "#f59e0b"],
          borderWidth: 0,
        },
      ],
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: {
          position: "bottom",
        },
        tooltip: {
          callbacks: {
            label: (context) => {
              const label = context.label || ""
              const value = context.raw || 0
              return `${label}: ${value}%`
            },
          },
        },
      },
    },
  })
// Replace the current gender chart (doughnut) with a line chart similar to the image
const interactionsChartCtx = document.getElementById("interactionsChart").getContext("2d")
const interactionsChart = new Chart(interactionsChartCtx, {
  type: "line",
  data: {
    labels: ["T1", "T2", "T3", "T4", "T5", "T6", "T7", "T8", "T9", "T10", "T11", "T12"],
    datasets: [
      {
        label: "Lượt xem",
        data: [1300, 1600, 2000, 2300, 1700, 2700, 3000, 2700, 3300, 3600, 4200, 3900],
        borderColor: "#3b82f6",
        backgroundColor: "rgba(59, 130, 246, 0.1)",
        tension: 0.4,
        borderWidth: 2,
        pointRadius: 4,
        pointBackgroundColor: "#3b82f6",
        fill: true
      },
      {
        label: "Lượt thích",
        data: [400, 500, 550, 600, 500, 650, 700, 650, 800, 900, 1000, 950],
        borderColor: "#10b981",
        backgroundColor: "transparent",
        tension: 0.4,
        borderWidth: 2,
        pointRadius: 4,
        pointBackgroundColor: "#10b981"
      },
      {
        label: "Bình luận",
        data: [250, 280, 300, 320, 300, 350, 380, 350, 400, 450, 500, 480],
        borderColor: "#f59e0b",
        backgroundColor: "transparent",
        tension: 0.4,
        borderWidth: 2,
        pointRadius: 4,
        pointBackgroundColor: "#f59e0b"
      },
      {
        label: "Chia sẻ",
        data: [120, 140, 160, 180, 150, 200, 220, 200, 240, 260, 280, 260],
        borderColor: "#8b5cf6",
        backgroundColor: "transparent",
        tension: 0.4,
        borderWidth: 2,
        pointRadius: 4,
        pointBackgroundColor: "#8b5cf6"
      }
    ],
  },
  options: {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        display: true,
        position: "bottom",
      },
      title: {
        display: true,
        text: "Tương tác người dùng theo thời gian",
        font: {
          size: 16,
          weight: 'bold'
        }
      },
      tooltip: {
        mode: "index",
        intersect: false,
      },
    },
    scales: {
      x: {
        grid: {
          display: true,
          color: "#f0f0f0"
        },
      },
      y: {
        beginAtZero: true,
        grid: {
          color: "#f0f0f0"
        },
        ticks: {
          callback: (value) => {
            if (value >= 1000) {
              return (value / 1000).toFixed(1) + "k"
            }
            return value
          },
        },
      }
    },
  },
})
  // Top 5 Posts Chart (Horizontal Bar Chart)
  const deviceChartCtx = document.getElementById("deviceChart").getContext("2d")
  const topPostsChart = new Chart(deviceChartCtx, {
    type: "bar",
    data: {
      labels: [
        "Khám phá Đà Lạt mùa hoa", 
        "Cẩm nang du lịch Hội An tự túc", 
        "Khám phá hang Sơn Đoòng", 
        "Trải nghiệm leo núi Fansipan", 
        "Bãi biển đẹp nhất Phú Quốc"
      ],
      datasets: [
        {
          label: "Lượt xem",
          data: [14000, 9800, 8500, 7200, 6000],
          backgroundColor: "#4a8af4", // Màu xanh dương như trong hình ảnh
          borderRadius: 4,
          barThickness: 30,
        },
      ],
    },
    options: {
      indexAxis: 'y',
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: {
          display: false,
        },
        title: {
          display: true,
          text: "Top 5 bài viết được xem nhiều nhất",
          font: {
            size: 16,
            weight: 'bold'
          }
        },
        tooltip: {
          callbacks: {
            label: (context) => {
              const value = context.raw || 0
              return `Lượt xem: ${value.toLocaleString()}`
            }
          }
        }
      },
      scales: {
        x: {
          grid: {
            display: true,
            color: "#f0f0f0",
            drawBorder: false
          },
          ticks: {
            stepSize: 3500,
            callback: function(value) {
              if (value >= 1000) {
                return value / 1000 + 'k';
              }
              return value;
            }
          },
          border: {
            display: false
          }
        },
        y: {
          grid: {
            display: false,
          },
          border: {
            display: false
          }
        },
      },
      layout: {
        padding: {
          top: 10,
          right: 20,
          bottom: 10,
          left: 10
        }
      }
    },
  })

  // Location Distribution Chart (Pie Chart) 
  const ageChartCtx = document.getElementById("ageChart").getContext("2d")
  const locationChart = new Chart(ageChartCtx, {
    type: "pie",
    data: {
      labels: ["Đà Lạt", "Hội An", "Quảng Bình", "Phú Quốc", "Hà Nội", "Đà Nẵng", "Hạ Long", "Nha Trang", "Huế", "Khác"],
      datasets: [
        {
          data: [18, 15, 12, 10, 8, 7, 6, 5, 4, 40],
          backgroundColor: [
            "#4a77ea", // Đà Lạt - xanh dương
            "#27c179", // Hội An - xanh lá
            "#f89e24", // Sapa - cam
            "#f35b9f", // Phú Quốc - hồng
            "#8f7ee6", // Hà Nội - tím nhạt
            "#f44336", // Đà Nẵng - đỏ
            "#00bcd4", // Hạ Long - xanh ngọc
            "#7e57c2", // Nha Trang - tím
            "#00acc1", // Huế - xanh lam
            "#78909c"  // Khác - xám
          ],
          borderWidth: 0,
        },
      ],
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: {
          position: "right",
          align: "center",
          labels: {
            usePointStyle: true,
            padding: 15,
            font: {
              size: 12
            }
          }
        },
        title: {
          display: true,
          text: "Phân bố bài viết theo địa điểm",
          font: {
            size: 16,
            weight: 'bold'
          }
        },
        tooltip: {
          callbacks: {
            label: (context) => {
              const label = context.label || ""
              const value = context.raw || 0
              return `${label}: ${value}`
            },
          },
        },
      },
    },
  })

function initializeTrafficChart() {
  const trafficChartCtx = document.getElementById("trafficChart").getContext("2d")
  window.trafficChart = new Chart(trafficChartCtx, {
    type: "line",
    data: {
      labels: ["T1", "T2", "T3", "T4", "T5", "T6", "T7", "T8", "T9", "T10", "T11", "T12"],
      datasets: [
        {
          label: "Lượt xem",
          data: [1200, 1800, 2100, 2400, 1800, 2800, 3100, 2800, 3400, 3800, 4300, 4000],
          borderColor: "#3b82f6",
          backgroundColor: "transparent",
          tension: 0.4,
          borderWidth: 2,
          pointRadius: 5,
          pointBackgroundColor: "#3b82f6"
        },
        {
          label: "Người dùng",
          data: [400, 450, 480, 520, 480, 600, 650, 620, 800, 900, 1000, 950],
          borderColor: "#10b981",
          backgroundColor: "transparent",
          tension: 0.4,
          borderWidth: 2,
          pointRadius: 5,
          pointBackgroundColor: "#10b981"
        },
        {
          label: "Bài viết",
          data: [15, 18, 22, 24, 20, 30, 33, 28, 40, 45, 48, 42],
          borderColor: "#f59e0b",
          backgroundColor: "transparent",
          tension: 0.4,
          borderWidth: 2,
          pointRadius: 5,
          pointBackgroundColor: "#f59e0b"
        }
      ],
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: {
          display: true,
          position: "bottom",
        },
        title: {
          display: true,
          text: "Lượt xem, người dùng và bài viết theo thời gian",
          font: {
            size: 16
          }
        },
        tooltip: {
          mode: "index",
          intersect: false,
        },
      },
      scales: {
        x: {
          grid: {
            display: true,
            color: "#f0f0f0"
          },
        },
        y: {
          beginAtZero: true,
          position: "left",
          grid: {
            color: "#f0f0f0"
          },
          ticks: {
            callback: (value) => {
              if (value >= 1000) {
                return (value / 1000).toFixed(1) + "k"
              }
              return value
            },
          },
        },
        y1: {
          position: "right",
          grid: {
            display: false,
          },
          beginAtZero: true,
          ticks: {
            callback: (value) => {
              return value
            },
          }
        }
      },
    },
  })
}

function updateTrafficChart(interval) {
  // In a real application, this would fetch new data based on the selected interval
  // For demo purposes, we'll just update the chart with sample data matching the year view
  let labels = [];
  let visitorsData = [];
  let newUsersData = [];
  let postsData = [];

  if (interval === "day") {
    labels = ["01/04", "05/04", "10/04", "15/04", "20/04", "25/04", "30/04"];
    visitorsData = [1500, 1700, 1850, 2100, 2300, 2450, 2600];
    newUsersData = [400, 450, 480, 520, 550, 580, 620];
    postsData = [15, 18, 20, 25, 28, 30, 32];
  } else if (interval === "week") {
    labels = ["Tuần 1", "Tuần 2", "Tuần 3", "Tuần 4"];
    visitorsData = [6500, 7200, 8100, 9000];
    newUsersData = [1800, 2000, 2200, 2400];
    postsData = [65, 72, 81, 90];
  } else if (interval === "month") {
    labels = ["T1", "T2", "T3", "T4", "T5", "T6", "T7", "T8", "T9", "T10", "T11", "T12"];
    visitorsData = [1200, 1800, 2100, 2400, 1800, 2800, 3100, 2800, 3400, 3800, 4300, 4000];
    newUsersData = [400, 450, 480, 520, 480, 600, 650, 620, 800, 900, 1000, 950];
    postsData = [15, 18, 22, 24, 20, 30, 33, 28, 40, 45, 48, 42];
  }

  window.trafficChart.data.labels = labels;
  window.trafficChart.data.datasets[0].data = visitorsData;
  window.trafficChart.data.datasets[1].data = newUsersData;
  
  // Make sure dataset exists before updating
  if (window.trafficChart.data.datasets[2]) {
    window.trafficChart.data.datasets[2].data = postsData;
  } else {
    window.trafficChart.data.datasets.push({
      label: "Bài viết",
      data: postsData,
      borderColor: "#f59e0b",
      backgroundColor: "transparent",
      tension: 0.4,
      borderWidth: 2,
      pointRadius: 5,
      pointBackgroundColor: "#f59e0b"
    });
  }
  
window.trafficChart.update();
}
}