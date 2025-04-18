document.addEventListener("DOMContentLoaded", () => {
  // Mobile sidebar toggle
  const toggleSidebarBtn = document.querySelector(".toggle-sidebar")
  if (toggleSidebarBtn) {
    toggleSidebarBtn.addEventListener("click", () => {
      document.querySelector(".sidebar").classList.toggle("open")
    })
  }

  // Report status filter
  const reportStatusSelect = document.getElementById("report-status")
  if (reportStatusSelect) {
    reportStatusSelect.addEventListener("change", function () {
      // In a real application, this would filter reports based on status
      console.log("Status filter changed to:", this.value)
    })
  }

  // View Report Buttons
  const viewButtons = document.querySelectorAll(".view-btn")
  const reportModal = document.getElementById("reportModal")
  if (viewButtons && reportModal) {
    viewButtons.forEach((button) => {
      button.addEventListener("click", () => {
        reportModal.classList.add("show")
      })
    })
  }

  // Cancel Action Button
  const cancelActionBtn = document.getElementById("cancelAction")
  if (cancelActionBtn) {
    cancelActionBtn.addEventListener("click", () => {
      reportModal.classList.remove("show")
    })
  }

  // Submit Action Button
  const submitActionBtn = document.getElementById("submitAction")
  if (submitActionBtn) {
    submitActionBtn.addEventListener("click", () => {
      const selectedAction = document.querySelector('input[name="action"]:checked')
      if (!selectedAction) {
        alert("Vui lòng chọn hành động xử lý!")
        return
      }

      const actionValue = selectedAction.value
      const actionNote = document.getElementById("action-note-text").value
      const reportStatus = document.getElementById("report-status-select").value

      // In a real application, this would send the action to the server
      console.log("Action:", actionValue)
      console.log("Note:", actionNote)
      console.log("Status:", reportStatus)

      alert("Đã xử lý báo cáo thành công!")
      reportModal.classList.remove("show")
    })
  }

  // Resolve Report Buttons
  const resolveButtons = document.querySelectorAll(".resolve-btn")
  if (resolveButtons) {
    resolveButtons.forEach((button) => {
      button.addEventListener("click", function () {
        const row = this.closest("tr")
        const statusCell = row.querySelector("td:nth-child(7)")

        // Update status
        statusCell.innerHTML = '<span class="badge green">Đã xử lý</span>'

        // Remove resolve and dismiss buttons
        this.remove()
        const dismissBtn = row.querySelector(".dismiss-btn")
        if (dismissBtn) dismissBtn.remove()

        alert("Báo cáo đã được đánh dấu là đã xử lý!")
      })
    })
  }

  // Dismiss Report Buttons
  const dismissButtons = document.querySelectorAll(".dismiss-btn")
  if (dismissButtons) {
    dismissButtons.forEach((button) => {
      button.addEventListener("click", function () {
        const row = this.closest("tr")
        const statusCell = row.querySelector("td:nth-child(7)")

        // Update status
        statusCell.innerHTML = '<span class="badge red">Đã bỏ qua</span>'

        // Remove resolve and dismiss buttons
        this.remove()
        const resolveBtn = row.querySelector(".resolve-btn")
        if (resolveBtn) resolveBtn.remove()

        alert("Báo cáo đã được đánh dấu là đã bỏ qua!")
      })
    })
  }

  // Report Status Change in Modal
  const reportStatusSelectModal = document.getElementById("report-status-select")
  if (reportStatusSelectModal) {
    reportStatusSelectModal.addEventListener("change", function () {
      // In a real application, this would update the report status
      console.log("Status changed to:", this.value)
    })
  }

  // Close modals when clicking outside
  window.addEventListener("click", (event) => {
    if (event.target === reportModal) {
      reportModal.classList.remove("show")
    }
  })

  // Close Modal Buttons
  const modalCloseButtons = document.querySelectorAll(".modal-close")
  if (modalCloseButtons) {
    modalCloseButtons.forEach((button) => {
      button.addEventListener("click", () => {
        document.querySelectorAll(".modal").forEach((modal) => {
          modal.classList.remove("show")
        })
      })
    })
  }
})
