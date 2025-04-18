document.addEventListener("DOMContentLoaded", () => {
  // Toggle Password Visibility
  const togglePasswordBtn = document.querySelector(".toggle-password")
  const passwordInput = document.getElementById("password")

  if (togglePasswordBtn && passwordInput) {
    togglePasswordBtn.addEventListener("click", () => {
      const type = passwordInput.getAttribute("type") === "password" ? "text" : "password"
      passwordInput.setAttribute("type", type)

      // Toggle eye icon
      const icon = togglePasswordBtn.querySelector("i")
      icon.classList.toggle("fa-eye")
      icon.classList.toggle("fa-eye-slash")
    })
  }

  // Login Form Submission
  const loginForm = document.getElementById("loginForm")
  const loginError = document.getElementById("loginError")

  if (loginForm) {
    loginForm.addEventListener("submit", (e) => {
      e.preventDefault()

      // Get form values
      const username = document.getElementById("username").value.trim()
      const password = document.getElementById("password").value
      const remember = document.getElementById("remember").checked

      // Simple validation
      if (!username || !password) {
        loginError.textContent = "Vui lòng nhập tên đăng nhập và mật khẩu."
        return
      }

      // Clear previous error
      loginError.textContent = ""

      // In a real application, this would send a request to authenticate the user
      // For demo purposes, we'll use a simple check
      if (username === "admin" && password === "admin123") {
        // Success - redirect to dashboard
        window.location.href = "index.html"
      } else {
        // Show error message
        loginError.textContent = "Tên đăng nhập hoặc mật khẩu không chính xác."
      }
    })
  }

  // Social Login Buttons
  const socialButtons = document.querySelectorAll(".btn-social")

  if (socialButtons.length > 0) {
    socialButtons.forEach((button) => {
      button.addEventListener("click", () => {
        // In a real application, this would redirect to the OAuth provider
        alert("Tính năng đăng nhập bằng mạng xã hội đang được phát triển.")
      })
    })
  }

  // Forgot Password Link
  const forgotPasswordLink = document.querySelector(".forgot-password")

  if (forgotPasswordLink) {
    forgotPasswordLink.addEventListener("click", (e) => {
      e.preventDefault()
      // In a real application, this would show a password reset form or redirect to a reset page
      alert("Tính năng khôi phục mật khẩu đang được phát triển.")
    })
  }
})
