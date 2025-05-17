document.addEventListener("DOMContentLoaded", () => {
  // Mobile sidebar toggle
  const toggleSidebarBtn = document.querySelector(".toggle-sidebar");
  if (toggleSidebarBtn) {
    toggleSidebarBtn.addEventListener("click", () => {
      document.querySelector(".sidebar").classList.toggle("open");
    });
  }

  // Select All Checkbox
  const selectAllCheckbox = document.getElementById("select-all");
  const userCheckboxes = document.querySelectorAll('input[id^="user-"]');

  if (selectAllCheckbox && userCheckboxes.length > 0) {
    selectAllCheckbox.addEventListener("change", function () {
      userCheckboxes.forEach((checkbox) => {
        checkbox.checked = this.checked;
      });
    });

    // Update select all checkbox when individual checkboxes change
    userCheckboxes.forEach((checkbox) => {
      checkbox.addEventListener("change", () => {
        const allChecked = Array.from(userCheckboxes).every(
          (checkbox) => checkbox.checked
        );
        const someChecked = Array.from(userCheckboxes).some(
          (checkbox) => checkbox.checked
        );

        selectAllCheckbox.checked = allChecked;
        selectAllCheckbox.indeterminate = someChecked && !allChecked;
      });
    });
  }

  // Add User Button
  const addUserBtn = document.querySelector(".btn-primary");
  const userModal = document.getElementById("userModal");
  const cancelUserBtn = document.getElementById("cancelUser");
  const modalCloseButtons = document.querySelectorAll(".modal-close");

  if (addUserBtn && userModal) {
    addUserBtn.addEventListener("click", () => {
      // Reset form
      document.getElementById("userForm").reset();
      document.querySelector(".modal-title").textContent =
        "Thêm người dùng mới";

      // Show modal
      userModal.classList.add("show");
    });
  }

  // Cancel User Button
  if (cancelUserBtn) {
    cancelUserBtn.addEventListener("click", () => {
      userModal.classList.remove("show");
    });
  }

  // Close Modal Buttons
  if (modalCloseButtons) {
    modalCloseButtons.forEach((button) => {
      button.addEventListener("click", () => {
        document.querySelectorAll(".modal").forEach((modal) => {
          modal.classList.remove("show");
        });
      });
    });
  }

  // Close modals when clicking outside
  window.addEventListener("click", (event) => {
    document.querySelectorAll(".modal").forEach((modal) => {
      if (event.target === modal) {
        modal.classList.remove("show");
      }
    });
  });

  // Toggle Password Visibility
  const togglePasswordButtons = document.querySelectorAll(".toggle-password");
  if (togglePasswordButtons) {
    togglePasswordButtons.forEach((button) => {
      button.addEventListener("click", function () {
        const passwordInput = this.previousElementSibling;
        const type =
          passwordInput.getAttribute("type") === "password"
            ? "text"
            : "password";
        passwordInput.setAttribute("type", type);

        // Toggle eye icon
        const icon = this.querySelector("i");
        icon.classList.toggle("fa-eye");
        icon.classList.toggle("fa-eye-slash");
      });
    });
  }

  // View User Buttons
  const viewButtons = document.querySelectorAll(".view-btn");
  if (viewButtons && userModal) {
    viewButtons.forEach((button) => {
      button.addEventListener("click", function () {
        const row = this.closest("tr");
        const name = row.querySelector(".user-name").textContent;
        const username = row
          .querySelector(".user-username")
          .textContent.substring(1); // Remove @ symbol
        const email = row.querySelector("td:nth-child(3)").textContent;
        const role = row.querySelector("td:nth-child(4) .badge").textContent;

        // Set modal title and populate form
        document.querySelector(".modal-title").textContent =
          "Chi tiết người dùng";
        document.getElementById("fullname").value = name;
        document.getElementById("username").value = username;
        document.getElementById("email").value = email;

        // Set role dropdown
        const roleSelect = document.getElementById("role");
        for (let i = 0; i < roleSelect.options.length; i++) {
          if (roleSelect.options[i].text === role) {
            roleSelect.selectedIndex = i;
            break;
          }
        }

        // Disable form fields for view mode
        const formElements = document.querySelectorAll(
          "#userForm input, #userForm select, #userForm textarea"
        );
        formElements.forEach((element) => {
          element.setAttribute("disabled", "disabled");
        });

        // Hide password fields
        document
          .getElementById("password")
          .closest(".form-group").style.display = "none";
        document
          .getElementById("confirm-password")
          .closest(".form-group").style.display = "none";

        // Show modal
        userModal.classList.add("show");
      });
    });
  }

  // Edit User Buttons
  const editButtons = document.querySelectorAll(".edit-btn");
  if (editButtons && userModal) {
    editButtons.forEach((button) => {
      button.addEventListener("click", function () {
        const row = this.closest("tr");
        const name = row.querySelector(".user-name").textContent;
        const username = row
          .querySelector(".user-username")
          .textContent.substring(1); // Remove @ symbol
        const email = row.querySelector("td:nth-child(3)").textContent;
        const role = row.querySelector("td:nth-child(4) .badge").textContent;

        // Set modal title and populate form
        document.querySelector(".modal-title").textContent =
          "Chỉnh sửa người dùng";
        document.getElementById("fullname").value = name;
        document.getElementById("username").value = username;
        document.getElementById("email").value = email;

        // Set role dropdown
        const roleSelect = document.getElementById("role");
        for (let i = 0; i < roleSelect.options.length; i++) {
          if (roleSelect.options[i].text === role) {
            roleSelect.selectedIndex = i;
            break;
          }
        }

        // Enable form fields for edit mode
        const formElements = document.querySelectorAll(
          "#userForm input, #userForm select, #userForm textarea"
        );
        formElements.forEach((element) => {
          element.removeAttribute("disabled");
        });

        // Show password fields but make them optional
        document
          .getElementById("password")
          .closest(".form-group").style.display = "block";
        document
          .getElementById("confirm-password")
          .closest(".form-group").style.display = "block";
        document.getElementById("password").removeAttribute("required");
        document.getElementById("confirm-password").removeAttribute("required");

        // Show modal
        userModal.classList.add("show");
      });
    });
  }

  // Delete User Buttons
  const deleteButtons = document.querySelectorAll(".delete-btn");
  const deleteModal = document.getElementById("deleteModal");
  const cancelDeleteBtn = document.getElementById("cancelDelete");
  const confirmDeleteBtn = document.getElementById("confirmDelete");

  if (deleteButtons && deleteModal) {
    deleteButtons.forEach((button) => {
      button.addEventListener("click", function () {
        const row = this.closest("tr");
        const name = row.querySelector(".user-name").textContent;

        // Set user name in confirmation message
        document.getElementById("deleteUserName").textContent = name;

        // Show delete confirmation modal
        deleteModal.classList.add("show");
      });
    });
  }

  // Cancel Delete Button
  if (cancelDeleteBtn) {
    cancelDeleteBtn.addEventListener("click", () => {
      deleteModal.classList.remove("show");
    });
  }

  // Confirm Delete Button
  if (confirmDeleteBtn) {
    confirmDeleteBtn.addEventListener("click", () => {
      // In a real application, this would send a request to delete the user
      alert("Người dùng đã được xóa thành công!");
      deleteModal.classList.remove("show");
    });
  }

  // User Form Submit
  const userForm = document.getElementById("userForm");
  if (userForm) {
    userForm.addEventListener("submit", (e) => {
      e.preventDefault();

      // Get form values
      const fullname = document.getElementById("fullname").value;
      const username = document.getElementById("username").value;
      const email = document.getElementById("email").value;
      const password = document.getElementById("password").value;
      const confirmPassword = document.getElementById("confirm-password").value;

      // Simple validation
      if (password !== confirmPassword) {
        alert("Mật khẩu và xác nhận mật khẩu không khớp!");
        return;
      }

      // In a real application, this would send the form data to the server
      alert("Người dùng đã được lưu thành công!");
      userModal.classList.remove("show");
    });
  }

  // Bulk Actions
  const bulkActionSelect = document.querySelector(
    ".bulk-actions .select-control"
  );
  const bulkActionButton = document.querySelector(
    ".bulk-actions .btn-secondary"
  );

  if (bulkActionSelect && bulkActionButton) {
    bulkActionButton.addEventListener("click", () => {
      const selectedAction = bulkActionSelect.value;
      if (!selectedAction) {
        alert("Vui lòng chọn hành động!");
        return;
      }

      const selectedUsers = Array.from(userCheckboxes).filter(
        (checkbox) => checkbox.checked
      );
      if (selectedUsers.length === 0) {
        alert("Vui lòng chọn ít nhất một người dùng!");
        return;
      }

      // In a real application, this would send a request to perform the bulk action
      alert(
        `Đã áp dụng hành động "${
          bulkActionSelect.options[bulkActionSelect.selectedIndex].text
        }" cho ${selectedUsers.length} người dùng!`
      );
    });
  }
});
