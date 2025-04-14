document.addEventListener("DOMContentLoaded", () => {
  // Mobile sidebar toggle
  const toggleSidebarBtn = document.querySelector(".toggle-sidebar")
  if (toggleSidebarBtn) {
    toggleSidebarBtn.addEventListener("click", () => {
      document.querySelector(".sidebar").classList.toggle("open")
    })
  }

  // Profile Tabs
  const tabButtons = document.querySelectorAll(".tab-btn")
  const tabContents = document.querySelectorAll(".tab-content")

  if (tabButtons.length > 0) {
    tabButtons.forEach((button) => {
      button.addEventListener("click", () => {
        // Remove active class from all tabs
        tabButtons.forEach((btn) => btn.classList.remove("active"))
        tabContents.forEach((content) => content.classList.remove("active"))

        // Add active class to clicked tab
        button.classList.add("active")
        const tabId = button.getAttribute("data-tab")
        document.getElementById(tabId).classList.add("active")
      })
    })
  }

  // Change Avatar Button
  const changeAvatarBtn = document.querySelector(".change-avatar-btn")
  if (changeAvatarBtn) {
    changeAvatarBtn.addEventListener("click", () => {
      // In a real application, this would open a file picker
      const fileInput = document.createElement("input")
      fileInput.type = "file"
      fileInput.accept = "image/*"
      fileInput.style.display = "none"
      document.body.appendChild(fileInput)

      fileInput.addEventListener("change", function () {
        if (this.files && this.files[0]) {
          // In a real application, this would upload the file to the server
          // For demo purposes, we'll just show a preview
          const reader = new FileReader()
          reader.onload = (e) => {
            document.querySelector(".profile-avatar img").src = e.target.result
          }
          reader.readAsDataURL(this.files[0])
        }
        document.body.removeChild(fileInput)
      })

      fileInput.click()
    })
  }

  // Change Cover Button
  const changeCoverBtn = document.querySelector(".change-cover-btn")
  if (changeCoverBtn) {
    changeCoverBtn.addEventListener("click", () => {
      // In a real application, this would open a file picker
      const fileInput = document.createElement("input")
      fileInput.type = "file"
      fileInput.accept = "image/*"
      fileInput.style.display = "none"
      document.body.appendChild(fileInput)

      fileInput.addEventListener("change", function () {
        if (this.files && this.files[0]) {
          // In a real application, this would upload the file to the server
          // For demo purposes, we'll just show a preview
          const reader = new FileReader()
          reader.onload = (e) => {
            document.querySelector(".profile-cover img").src = e.target.result
          }
          reader.readAsDataURL(this.files[0])
        }
        document.body.removeChild(fileInput)
      })

      fileInput.click()
    })
  }

  // Edit Profile Button
  const editProfileBtn = document.querySelector(".profile-actions .btn-primary")
  if (editProfileBtn) {
    editProfileBtn.addEventListener("click", () => {
      // Switch to the info tab
      document.querySelector('.tab-btn[data-tab="info"]').click()

      // Focus on the first input
      document.getElementById("fullname").focus()
    })
  }

  // Save Changes Button
  const saveChangesBtn = document.querySelector(".form-actions .btn-primary")
  if (saveChangesBtn) {
    saveChangesBtn.addEventListener("click", () => {
      // In a real application, this would save the form data to the server
      alert("Thông tin cá nhân đã được cập nhật thành công!")
    })
  }

  // Cancel Button
  const cancelBtn = document.querySelector(".form-actions .btn-secondary")
  if (cancelBtn) {
    cancelBtn.addEventListener("click", () => {
      // In a real application, this would reset the form to the original values
      if (confirm("Bạn có chắc chắn muốn hủy các thay đổi?")) {
        window.location.reload()
      }
    })
  }

  // Change Password Button
  const changePasswordBtn = document.querySelector("#security .btn-primary")
  if (changePasswordBtn) {
    changePasswordBtn.addEventListener("click", () => {
      const currentPassword = document.getElementById("current-password").value
      const newPassword = document.getElementById("new-password").value
      const confirmPassword = document.getElementById("confirm-password").value

      // Simple validation
      if (!currentPassword || !newPassword || !confirmPassword) {
        alert("Vui lòng điền đầy đủ thông tin!")
        return
      }

      if (newPassword !== confirmPassword) {
        alert("Mật khẩu mới và xác nhận mật khẩu không khớp!")
        return
      }

      // In a real application, this would send a request to change the password
      alert("Mật khẩu đã được thay đổi thành công!")

      // Clear the form
      document.getElementById("current-password").value = ""
      document.getElementById("new-password").value = ""
      document.getElementById("confirm-password").value = ""
    })
  }

  // Two Factor Auth Toggle
  const twoFactorToggle = document.getElementById("two-factor")
  if (twoFactorToggle) {
    twoFactorToggle.addEventListener("change", function () {
      // In a real application, this would enable/disable two-factor authentication
      if (this.checked) {
        alert("Xác thực hai yếu tố đã được bật. Vui lòng thiết lập phương thức xác thực.")
      } else {
        alert("Xác thực hai yếu tố đã được tắt.")
      }
    })
  }

  // Two Factor Auth Setup Buttons
  const setupButtons = document.querySelectorAll(".auth-method .btn-secondary")
  if (setupButtons.length > 0) {
    setupButtons.forEach((button) => {
      button.addEventListener("click", function () {
        const methodName = this.closest(".auth-method").querySelector("h5").textContent
        alert(`Thiết lập ${methodName} đang được phát triển.`)
      })
    })
  }

  // Session End Buttons
  const sessionEndButtons = document.querySelectorAll(".session-actions .btn-text")
  if (sessionEndButtons.length > 0) {
    sessionEndButtons.forEach((button) => {
      button.addEventListener("click", function () {
        const sessionItem = this.closest(".session-item")
        const deviceName = sessionItem.querySelector("h5").textContent

        if (sessionItem.classList.contains("current")) {
          if (confirm(`Bạn có chắc chắn muốn kết thúc phiên hiện tại trên ${deviceName}? Bạn sẽ bị đăng xuất.`)) {
            // In a real application, this would end the current session and redirect to login
            window.location.href = "login.html"
          }
        } else {
          if (confirm(`Bạn có chắc chắn muốn kết thúc phiên trên ${deviceName}?`)) {
            // In a real application, this would end the session
            sessionItem.remove()
            alert(`Đã kết thúc phiên trên ${deviceName}.`)
          }
        }
      })
    })
  }

  // End All Other Sessions Button
  const endAllSessionsBtn = document.querySelector("#security .btn-danger")
  if (endAllSessionsBtn) {
    endAllSessionsBtn.addEventListener("click", () => {
      if (confirm("Bạn có chắc chắn muốn kết thúc tất cả các phiên khác?")) {
        // In a real application, this would end all other sessions
        const otherSessions = document.querySelectorAll(".session-item:not(.current)")
        otherSessions.forEach((session) => {
          session.remove()
        })
        alert("Đã kết thúc tất cả các phiên khác.")
      }
    })
  }
})
