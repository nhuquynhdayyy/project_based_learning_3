// Common JavaScript functions used across multiple pages

// Toggle sidebar on mobile
function toggleSidebar() {
  const sidebar = document.querySelector(".sidebar")
  if (sidebar) {
    sidebar.classList.toggle("open")
  }
}

// Initialize mobile sidebar toggle
function initMobileSidebar() {
  const toggleBtn = document.querySelector(".toggle-sidebar")
  if (toggleBtn) {
    toggleBtn.addEventListener("click", toggleSidebar)
  }
}

// Handle logout functionality
function handleLogout() {
  const logoutBtn = document.querySelector(".logout-button")
  if (logoutBtn) {
    logoutBtn.addEventListener("click", () => {
      if (confirm("Bạn có chắc chắn muốn đăng xuất?")) {
        // In a real application, this would clear session data
        window.location.href = "login.html"
      }
    })
  }
}

// Toggle dark mode
function toggleDarkMode(isDark) {
  const body = document.body
  if (isDark) {
    body.classList.add("dark-mode")
    localStorage.setItem("darkMode", "true")
  } else {
    body.classList.remove("dark-mode")
    localStorage.setItem("darkMode", "false")
  }
}

// Check for saved dark mode preference
function checkDarkModePreference() {
  const darkModeToggle = document.getElementById("darkModeToggle")
  const savedDarkMode = localStorage.getItem("darkMode")

  if (savedDarkMode === "true") {
    document.body.classList.add("dark-mode")
    if (darkModeToggle) {
      darkModeToggle.checked = true
    }
  }
}

// Initialize common functionality
document.addEventListener("DOMContentLoaded", () => {
  initMobileSidebar()
  handleLogout()
  checkDarkModePreference()
})
