// dashboard.js - Script cho trang Dashboard

document.addEventListener('DOMContentLoaded', function() {
  // Khởi tạo biểu đồ khi trang đã tải xong
  initializeCharts();

  // Xử lý sự kiện bật/tắt sidebar trên mobile
  const toggleSidebarButton = document.querySelector('.toggle-sidebar');
  const appContainer = document.querySelector('.app-container');
  
  if (toggleSidebarButton) {
      toggleSidebarButton.addEventListener('click', function() {
          appContainer.classList.toggle('sidebar-collapsed');
      });
  }

  // Xử lý sự kiện khi thay đổi khoảng thời gian
  const timeRangeSelect = document.getElementById('time-range');
  if (timeRangeSelect) {
      timeRangeSelect.addEventListener('change', function() {
          updateChartsData(this.value);
      });
  }
});

// Khởi tạo các biểu đồ
function initializeCharts() {
  // Biểu đồ cột nhóm cho số bài viết theo loại
  const postsChartCtx = document.getElementById('postsChart');
  if (postsChartCtx) {
      // Dữ liệu mẫu cho biểu đồ cột
      const barChartData = {
          labels: ['1/4', '8/4', '15/4', '22/4', '29/4'],
          datasets: [
              {
                  label: 'Địa điểm',
                  data: [4, 3, 7, 5, 6],
                  backgroundColor: 'rgb(66, 133, 244)',
                  barPercentage: 0.7,
                  categoryPercentage: 0.8
              },
              {
                  label: 'Cẩm nang',
                  data: [3, 4, 2, 6, 3],
                  backgroundColor: 'rgb(52, 168, 83)',
                  barPercentage: 0.7,
                  categoryPercentage: 0.8
              },
              {
                  label: 'Trải nghiệm',
                  data: [2, 3, 4, 3, 5],
                  backgroundColor: 'rgb(251, 188, 5)',
                  barPercentage: 0.7,
                  categoryPercentage: 0.8
              }
          ]
      };

      new Chart(postsChartCtx, {
          type: 'bar',
          data: barChartData,
          options: {
              responsive: true,
              maintainAspectRatio: false,
              plugins: {
                  legend: {
                      position: 'bottom',
                      labels: {
                          boxWidth: 12,
                          padding: 15,
                          font: {
                              size: 12
                          }
                      }
                  },
                  tooltip: {
                      mode: 'index',
                      intersect: false,
                      backgroundColor: 'rgba(255, 255, 255, 0.9)',
                      titleColor: '#333',
                      bodyColor: '#333',
                      borderColor: '#ddd',
                      borderWidth: 1,
                      titleFont: {
                          size: 14,
                          weight: 'bold'
                      },
                      bodyFont: {
                          size: 13
                      },
                      padding: 12,
                      usePointStyle: true
                  }
              },
              scales: {
                  x: {
                      grid: {
                          drawBorder: false,
                          display: false
                      },
                      ticks: {
                          font: {
                              size: 12
                          }
                      }
                  },
                  y: {
                      beginAtZero: true,
                      max: 8,
                      grid: {
                          color: 'rgba(200, 200, 200, 0.3)',
                          drawBorder: false,
                          borderDash: [3, 3]
                      },
                      ticks: {
                          stepSize: 2,
                          font: {
                              size: 12
                          }
                      }
                  }
              }
          }
      });
  }

  // Biểu đồ tròn cho tỷ lệ phân bố bài viết
  const distributionChartCtx = document.getElementById('distributionChart');
  if (distributionChartCtx) {
      const pieChartData = {
          labels: ['Địa điểm', 'Cẩm nang', 'Trải nghiệm'],
          datasets: [{
              data: [40, 35, 25],
              backgroundColor: [
                  'rgb(66, 133, 244)',  // Xanh dương
                  'rgb(52, 168, 83)',   // Xanh lá
                  'rgb(251, 188, 5)'    // Vàng
              ],
              borderWidth: 0
          }]
      };

      new Chart(distributionChartCtx, {
          type: 'pie',
          data: pieChartData,
          options: {
              responsive: true,
              maintainAspectRatio: false,
              plugins: {
                  legend: {
                      position: 'right',
                      align: 'center',
                      labels: {
                          boxWidth: 0,
                          font: {
                              size: 12
                          },
                          generateLabels: function(chart) {
                              const data = chart.data;
                              if (data.labels.length && data.datasets.length) {
                                  return data.labels.map(function(label, i) {
                                      const value = data.datasets[0].data[i];
                                      return {
                                          text: `${label}: ${value}%`,
                                          fillStyle: data.datasets[0].backgroundColor[i],
                                          index: i
                                      };
                                  });
                              }
                              return [];
                          }
                      }
                  },
                  tooltip: {
                      backgroundColor: 'rgba(255, 255, 255, 0.9)',
                      titleColor: '#333',
                      bodyColor: '#333',
                      borderColor: '#ddd',
                      borderWidth: 1,
                      callbacks: {
                          label: function(context) {
                              const label = context.label || '';
                              const value = context.formattedValue;
                              return `${label}: ${value}%`;
                          }
                      }
                  }
              }
          }
      });
  }
}

// Cập nhật dữ liệu biểu đồ khi thay đổi khoảng thời gian
function updateChartsData(timeRange) {
  // Tùy chỉnh dữ liệu biểu đồ theo khoảng thời gian đã chọn
  let barData, pieData;
  
  if (timeRange === '7') {
      // Dữ liệu cho 7 ngày
      barData = {
          labels: ['23/4', '24/4', '25/4', '26/4', '27/4', '28/4', '29/4'],
          datasets: [
              {
                  label: 'Địa điểm',
                  data: [3, 4, 5, 4, 3, 5, 6],
                  backgroundColor: 'rgb(66, 133, 244)'
              },
              {
                  label: 'Cẩm nang',
                  data: [2, 3, 4, 5, 2, 3, 3],
                  backgroundColor: 'rgb(52, 168, 83)'
              },
              {
                  label: 'Trải nghiệm',
                  data: [1, 2, 3, 2, 4, 3, 5],
                  backgroundColor: 'rgb(251, 188, 5)'
              }
          ]
      };
      
      pieData = [45, 30, 25]; // Thay đổi tỷ lệ cho 7 ngày
  } else if (timeRange === '30') {
      // Giữ nguyên dữ liệu mặc định cho 30 ngày
      barData = {
          labels: ['1/4', '8/4', '15/4', '22/4', '29/4'],
          datasets: [
              {
                  label: 'Địa điểm',
                  data: [4, 3, 7, 5, 6],
                  backgroundColor: 'rgb(66, 133, 244)'
              },
              {
                  label: 'Cẩm nang',
                  data: [3, 4, 2, 6, 3],
                  backgroundColor: 'rgb(52, 168, 83)'
              },
              {
                  label: 'Trải nghiệm',
                  data: [2, 3, 4, 3, 5],
                  backgroundColor: 'rgb(251, 188, 5)'
              }
          ]
      };
      
      pieData = [40, 35, 25]; // Tỷ lệ cho 30 ngày
  } else {
      // Dữ liệu cho "tất cả" (giả sử hiển thị theo tháng)
      barData = {
          labels: ['Tháng 1', 'Tháng 2', 'Tháng 3', 'Tháng 4'],
          datasets: [
              {
                  label: 'Địa điểm',
                  data: [25, 30, 35, 48],
                  backgroundColor: 'rgb(66, 133, 244)'
              },
              {
                  label: 'Cẩm nang',
                  data: [20, 25, 30, 42],
                  backgroundColor: 'rgb(52, 168, 83)'
              },
              {
                  label: 'Trải nghiệm',
                  data: [15, 22, 28, 35],
                  backgroundColor: 'rgb(251, 188, 5)'
              }
          ]
      };
      
      pieData = [38, 34, 28]; // Tỷ lệ cho "tất cả"
  }
  
  updateBarChart(barData);
  updatePieChart(pieData);
}

// Cập nhật biểu đồ cột
function updateBarChart(newData) {
  const barChart = Chart.getChart('postsChart');
  if (barChart) {
      barChart.data.labels = newData.labels;
      barChart.data.datasets.forEach((dataset, i) => {
          dataset.data = newData.datasets[i].data;
      });
      barChart.update();
  }
}

// Cập nhật biểu đồ tròn
function updatePieChart(newData) {
  const pieChart = Chart.getChart('distributionChart');
  if (pieChart) {
      pieChart.data.datasets[0].data = newData;
      pieChart.update();
  }
}