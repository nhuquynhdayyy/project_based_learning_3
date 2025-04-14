document.addEventListener("DOMContentLoaded", () => {
    // Mobile sidebar toggle
    const toggleSidebarBtn = document.querySelector(".toggle-sidebar")
    if (toggleSidebarBtn) {
      toggleSidebarBtn.addEventListener("click", () => {
        document.querySelector(".sidebar").classList.toggle("open")
      })
    }
  
    // View Comment Buttons
    const viewButtons = document.querySelectorAll(".view-btn")
    const commentModal = document.getElementById("commentModal")
    if (viewButtons && commentModal) {
      viewButtons.forEach((button) => {
        button.addEventListener("click", () => {
          commentModal.classList.add("show")
        })
      })
    }
  
    // Approve Comment Buttons
    const approveButtons = document.querySelectorAll(".approve-btn")
    if (approveButtons) {
      approveButtons.forEach((button) => {
        button.addEventListener("click", function () {
          const row = this.closest("tr")
          const statusCell = row.querySelector("td:nth-child(6)")
  
          // Update status
          statusCell.innerHTML = '<span class="badge green">Đã duyệt</span>'
  
          // Remove approve and reject buttons
          this.remove()
          const rejectBtn = row.querySelector(".reject-btn")
          if (rejectBtn) rejectBtn.remove()
  
          alert("Bình luận đã được duyệt!")
        })
      })
    }
  
    // Reject Comment Buttons
    const rejectButtons = document.querySelectorAll(".reject-btn")
    if (rejectButtons) {
      rejectButtons.forEach((button) => {
        button.addEventListener("click", function () {
          const row = this.closest("tr")
          const statusCell = row.querySelector("td:nth-child(6)")
  
          // Update status
          statusCell.innerHTML = '<span class="badge red">Từ chối</span>'
  
          // Remove approve and reject buttons
          this.remove()
          const approveBtn = row.querySelector(".approve-btn")
          if (approveBtn) approveBtn.remove()
  
          alert("Bình luận đã bị từ chối!")
        })
      })
    }
  
    // Delete Comment Buttons
    const deleteButtons = document.querySelectorAll(".delete-btn")
    const deleteModal = document.getElementById("deleteModal")
    const cancelDeleteBtn = document.getElementById("cancelDelete")
    const confirmDeleteBtn = document.getElementById("confirmDelete")
  
    if (deleteButtons && deleteModal) {
      deleteButtons.forEach((button) => {
        button.addEventListener("click", () => {
          deleteModal.classList.add("show")
        })
      })
    }
  
    if (cancelDeleteBtn) {
      cancelDeleteBtn.addEventListener("click", () => {
        deleteModal.classList.remove("show")
      })
    }
  
    if (confirmDeleteBtn) {
      confirmDeleteBtn.addEventListener("click", () => {
        // In a real application, this would send a request to delete the comment
        alert("Bình luận đã được xóa thành công!")
        deleteModal.classList.remove("show")
  
        // For demo purposes, remove a random row
        const rows = document.querySelectorAll(".data-table tbody tr")
        if (rows.length > 0) {
          const randomIndex = Math.floor(Math.random() * rows.length)
          rows[randomIndex].remove()
        }
      })
    }
  
    // Modal Actions
    const approveCommentBtn = document.getElementById("approveComment")
    const rejectCommentBtn = document.getElementById("rejectComment")
    const cancelCommentBtn = document.getElementById("cancelComment")
  
    if (approveCommentBtn) {
      approveCommentBtn.addEventListener("click", () => {
        // In a real application, this would send a request to approve the comment
        alert("Bình luận đã được duyệt!")
        commentModal.classList.remove("show")
      })
    }
  
    if (rejectCommentBtn) {
      rejectCommentBtn.addEventListener("click", () => {
        // In a real application, this would send a request to reject the comment
        alert("Bình luận đã bị từ chối!")
        commentModal.classList.remove("show")
      })
    }
  
    if (cancelCommentBtn) {
      cancelCommentBtn.addEventListener("click", () => {
        commentModal.classList.remove("show")
      })
    }
  
    // Close modals when clicking outside
    window.addEventListener("click", (event) => {
      if (event.target === commentModal) {
        commentModal.classList.remove("show")
      }
      if (event.target === deleteModal) {
        deleteModal.classList.remove("show")
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
  