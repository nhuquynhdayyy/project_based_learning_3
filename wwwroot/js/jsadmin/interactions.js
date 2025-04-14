import { Chart } from "@/components/ui/chart"
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

  // Initialize charts
  initializeCharts()
})

function initializeCharts() {
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
  // Distribution Chart (Doughnut Chart)
  const distributionChartCtx = document.getElementById("distributionChart").getContext("2d")
  const distributionChart = new Chart(distributionChartCtx, {
    type: "doughnut",
    data: {
      labels: ["Lượt xem", "Lượt thích", "Lượt lưu", "Lượt chia sẻ"],
      datasets: [
        {
          data: [24582, 1248, 856, 432],
          backgroundColor: ["#3b82f6", "#ef4444", "#f59e0b", "#10b981"],
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
              const total = context.dataset.data.reduce((a, b) => a + b, 0)
              const percentage = Math.round((value / total) * 100)
              return `${label}: ${value.toLocaleString()} (${percentage}%)`
            },
          },
        },
      },
      cutout: "65%",
    },
  })
}
