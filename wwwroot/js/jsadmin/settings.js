document.addEventListener("DOMContentLoaded", () => {
  // Settings Tabs
  const tabButtons = document.querySelectorAll(".settings-nav-item")
  const tabContents = document.querySelectorAll(".settings-tab")

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

  // Save Changes Buttons
  const saveButtons = document.querySelectorAll(".btn-primary")
  if (saveButtons.length > 0) {
    saveButtons.forEach((button) => {
      button.addEventListener("click", () => {
        // In a real application, this would save the settings to the server
        alert("Cài đặt đã được lưu thành công!")
      })
    })
  }

  // Reset Default Buttons
  const resetButtons = document.querySelectorAll(".form-actions .btn-secondary")
  if (resetButtons.length > 0) {
    resetButtons.forEach((button) => {
      button.addEventListener("click", () => {
        if (confirm("Bạn có chắc chắn muốn khôi phục cài đặt mặc định?")) {
          // In a real application, this would reset the settings to default values
          alert("Cài đặt đã được khôi phục về mặc định!")
        }
      })
    })
  }

  // Theme Options
  const themeOptions = document.querySelectorAll('input[name="theme"]')
  if (themeOptions.length > 0) {
    themeOptions.forEach((option) => {
      option.addEventListener("change", function () {
        if (this.value === "dark") {
          document.body.classList.add("dark-mode")
          localStorage.setItem("theme", "dark")
        } else if (this.value === "light") {
          document.body.classList.remove("dark-mode")
          localStorage.setItem("theme", "light")
        } else {
          // System preference
          if (window.matchMedia && window.matchMedia("(prefers-color-scheme: dark)").matches) {
            document.body.classList.add("dark-mode")
          } else {
            document.body.classList.remove("dark-mode")
          }
          localStorage.setItem("theme", "system")
        }
      })
    })
  }

  // Check for saved theme preference
  const savedTheme = localStorage.getItem("theme")
  if (savedTheme) {
    if (savedTheme === "dark") {
      document.body.classList.add("dark-mode")
      const darkThemeRadio = document.getElementById("theme-dark")
      if (darkThemeRadio) darkThemeRadio.checked = true
    } else if (savedTheme === "system") {
      const systemThemeRadio = document.getElementById("theme-system")
      if (systemThemeRadio) systemThemeRadio.checked = true

      if (window.matchMedia && window.matchMedia("(prefers-color-scheme: dark)").matches) {
        document.body.classList.add("dark-mode")
      }
    }
  }

  // Color Pickers
  const colorPickers = document.querySelectorAll('input[type="color"]')
  if (colorPickers.length > 0) {
    colorPickers.forEach((picker) => {
      const textInput = picker.nextElementSibling

      // Update text input when color picker changes
      picker.addEventListener("input", () => {
        textInput.value = picker.value
      })

      // Update color picker when text input changes
      textInput.addEventListener("input", () => {
        // Validate hex color
        if (/^#[0-9A-F]{6}$/i.test(textInput.value)) {
          picker.value = textInput.value
        }
      })
    })
  }

  // File Upload
  const fileInput = document.getElementById("restore-file")
  const restoreButton = document.querySelector(".file-upload + .btn")

  if (fileInput && restoreButton) {
    fileInput.addEventListener("change", function () {
      if (this.files.length > 0) {
        const fileName = this.files[0].name
        this.nextElementSibling.querySelector("span").textContent = fileName
        restoreButton.removeAttribute("disabled")
      } else {
        this.nextElementSibling.querySelector("span").textContent = "Chọn tệp sao lưu"
        restoreButton.setAttribute("disabled", "disabled")
      }
    })

    restoreButton.addEventListener("click", () => {
      if (fileInput.files.length > 0) {
        if (confirm("Bạn có chắc chắn muốn khôi phục dữ liệu từ bản sao lưu này? Dữ liệu hiện tại sẽ bị ghi đè.")) {
          // In a real application, this would upload the file and restore the data
          alert("Đã khôi phục dữ liệu thành công!")
        }
      }
    })
  }

  // Backup Actions
  const backupButtons = document.querySelectorAll(".backup-actions .btn-icon")
  if (backupButtons.length > 0) {
    backupButtons.forEach((button) => {
      button.addEventListener("click", function () {
        const action = this.getAttribute("title")
        const backupItem = this.closest(".backup-item")
        const backupName = backupItem.querySelector(".backup-name span").textContent

        if (action === "Tải xuống") {
          // In a real application, this would download the backup file
          alert(`Đang tải xuống bản sao lưu: ${backupName}`)
        } else if (action === "Khôi phục") {
          if (
            confirm(
              `Bạn có chắc chắn muốn khôi phục dữ liệu từ bản sao lưu: ${backupName}? Dữ liệu hiện tại sẽ bị ghi đè.`,
            )
          ) {
            // In a real application, this would restore the data from the backup
            alert(`Đã khôi phục dữ liệu từ bản sao lưu: ${backupName}`)
          }
        } else if (action === "Xóa") {
          if (confirm(`Bạn có chắc chắn muốn xóa bản sao lưu: ${backupName}?`)) {
            // In a real application, this would delete the backup
            backupItem.remove()
            alert(`Đã xóa bản sao lưu: ${backupName}`)
          }
        }
      })
    })
  }

  // Create Backup Button
  const createBackupButton = document.querySelector(".backup-options + .btn-primary")
  if (createBackupButton) {
    createBackupButton.addEventListener("click", () => {
      // In a real application, this would create a backup
      alert("Đang tạo bản sao lưu... Quá trình này có thể mất vài phút.")

      // Simulate backup creation
      setTimeout(() => {
        alert("Đã tạo bản sao lưu thành công!")
      }, 2000)
    })
  }

  // Create API Key Button
  const createApiKeyButton = document.querySelector(".api-keys + .btn-primary")
  if (createApiKeyButton) {
    createApiKeyButton.addEventListener("click", () => {
      const name = prompt("Nhập tên cho API Key mới:")
      if (name) {
        // In a real application, this would create a new API key
        alert(`Đã tạo API Key mới: ${name}`)
      }
    })
  }

  // API Key Actions
  const apiKeyButtons = document.querySelectorAll(".api-key-actions .btn")
  if (apiKeyButtons.length > 0) {
    apiKeyButtons.forEach((button) => {
      button.addEventListener("click", function () {
        const action = this.textContent.trim()
        const apiKeyItem = this.closest(".api-key-item")
        const apiKeyName = apiKeyItem.querySelector(".api-key-name span").textContent

        if (action.includes("Tạo lại")) {
          if (confirm(`Bạn có chắc chắn muốn tạo lại API Key cho: ${apiKeyName}? API Key cũ sẽ không còn hoạt động.`)) {
            // In a real application, this would regenerate the API key
            const newKey =
              "sk_live_" + Math.random().toString(36).substring(2, 15) + Math.random().toString(36).substring(2, 15)
            apiKeyItem.querySelector(".api-key-value code").textContent = newKey
            alert(`Đã tạo lại API Key cho: ${apiKeyName}`)
          }
        } else if (action.includes("Xóa")) {
          if (confirm(`Bạn có chắc chắn muốn xóa API Key: ${apiKeyName}?`)) {
            // In a real application, this would delete the API key
            apiKeyItem.remove()
            alert(`Đã xóa API Key: ${apiKeyName}`)
          }
        }
      })
    })
  }

  // Integration Configuration Buttons
  const configButtons = document.querySelectorAll(".integration-actions .btn-text")
  if (configButtons.length > 0) {
    configButtons.forEach((button) => {
      button.addEventListener("click", function () {
        const integrationItem = this.closest(".integration-item")
        const integrationName = integrationItem.querySelector("h4").textContent

        // In a real application, this would open a configuration modal
        alert(`Đang mở cấu hình cho: ${integrationName}`)
      })
    })
  }

  // System Log Controls
  const logControls = document.querySelectorAll(".log-controls .btn, .log-controls select")
  if (logControls.length > 0) {
    logControls.forEach((control) => {
      control.addEventListener("click", function () {
        if (this.tagName === "BUTTON") {
          const action = this.querySelector("i").className

          if (action.includes("sync-alt")) {
            // In a real application, this would refresh the logs
            alert("Đang làm mới nhật ký hệ thống...")
          } else if (action.includes("download")) {
            // In a real application, this would download the logs
            alert("Đang tải xuống nhật ký hệ thống...")
          }
        }
      })
    })
  }

  // System Info Buttons
  const systemInfoButtons = document.querySelectorAll(".system-info .btn")
  if (systemInfoButtons.length > 0) {
    systemInfoButtons.forEach((button) => {
      button.addEventListener("click", function () {
        const action = this.textContent.trim()

        if (action.includes("Kiểm tra cập nhật")) {
          // In a real application, this would check for updates
          alert("Đang kiểm tra cập nhật...")

          // Simulate update check
          setTimeout(() => {
            alert("Hệ thống của bạn đã được cập nhật đến phiên bản mới nhất!")
          }, 1500)
        } else if (action.includes("Tối ưu hóa")) {
          // In a real application, this would optimize the database
          alert("Đang tối ưu hóa cơ sở dữ liệu...")

          // Simulate optimization
          setTimeout(() => {
            alert("Đã tối ưu hóa cơ sở dữ liệu thành công!")
          }, 2000)
        }
      })
    })
  }
})
