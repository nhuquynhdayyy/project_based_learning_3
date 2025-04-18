document.addEventListener('DOMContentLoaded', function() {
  // Elements
  const filterButton = document.getElementById('filter-button');
  const postTypeFilter = document.getElementById('post-type-filter');
  const postStatusFilter = document.getElementById('post-status-filter');
  const postLocationFilter = document.getElementById('post-location-filter');
  const searchInput = document.querySelector('.search-input input');
  const dataRows = document.querySelectorAll('.data-row');
  const totalItemsElement = document.getElementById('total-items');
  const currentPageElement = document.getElementById('current-page');
  const totalPagesElement = document.getElementById('total-pages');
  const prevPageButton = document.getElementById('prev-page');
  const nextPageButton = document.getElementById('next-page');
  const itemsPerPageSelect = document.getElementById('items-per-page');
  const addPostButton = document.querySelector('.btn-primary');
  const postModal = document.getElementById('postModal');
  const deleteModal = document.getElementById('deleteModal');
  const modalCloseButtons = document.querySelectorAll('.modal-close');
  const cancelPostButton = document.getElementById('cancelPost');
  const cancelDeleteButton = document.getElementById('cancelDelete');
  const confirmDeleteButton = document.getElementById('confirmDelete');
  const toggleSidebarButton = document.querySelector('.toggle-sidebar');
  const sidebar = document.querySelector('.sidebar');
  
  // Pagination state
  let currentPage = 1;
  let itemsPerPage = 5;
  let filteredRows = [...dataRows];
  let totalPages = Math.ceil(filteredRows.length / itemsPerPage);
  
  // Initialize page
  updatePagination();
  
  // Event Listeners
  filterButton.addEventListener('click', applyFilters);
  searchInput.addEventListener('input', applyFilters);
  postTypeFilter.addEventListener('change', applyFilters);
  postStatusFilter.addEventListener('change', applyFilters);
  postLocationFilter.addEventListener('change', applyFilters);
  prevPageButton.addEventListener('click', goToPrevPage);
  nextPageButton.addEventListener('click', goToNextPage);
  itemsPerPageSelect.addEventListener('change', changeItemsPerPage);
  addPostButton.addEventListener('click', openPostModal);
  
  // Close modals
  modalCloseButtons.forEach(button => {
      button.addEventListener('click', () => {
          postModal.classList.remove('active');
          deleteModal.classList.remove('active');
      });
  });
  
  cancelPostButton.addEventListener('click', () => {
      postModal.classList.remove('active');
  });
  
  cancelDeleteButton.addEventListener('click', () => {
      deleteModal.classList.remove('active');
  });
  
  // Handle delete button clicks
  document.querySelectorAll('.delete-btn').forEach(button => {
      button.addEventListener('click', (e) => {
          e.preventDefault();
          const row = button.closest('tr');
          const title = row.querySelector('.table-title-cell').textContent;
          document.getElementById('deletePostTitle').textContent = title;
          deleteModal.classList.add('active');
          
          // Save the row to delete
          confirmDeleteButton.dataset.rowId = row.cells[0].textContent;
      });
  });
  
  // Handle confirm delete
  confirmDeleteButton.addEventListener('click', () => {
      const rowId = confirmDeleteButton.dataset.rowId;
      const rowToDelete = document.querySelector(`tr td:first-child:contains('${rowId}')`).closest('tr');
      if (rowToDelete) {
          rowToDelete.remove();
          updatePagination();
      }
      deleteModal.classList.remove('active');
  });
  
  // Toggle sidebar on mobile
  toggleSidebarButton.addEventListener('click', () => {
      sidebar.classList.toggle('active');
  });
  
  // Handle view and edit buttons
  document.querySelectorAll('.view-btn, .edit-btn').forEach(button => {
      button.addEventListener('click', (e) => {
          e.preventDefault();
          const isEdit = button.classList.contains('edit-btn');
          const row = button.closest('tr');
          const title = row.querySelector('.table-title-cell').textContent;
          
          // Fill the form with post data if it's edit
          if (isEdit) {
              document.getElementById('title').value = title;
              // In a real application, you would fetch the full post data from the server
              // and fill all form fields
              
              document.querySelector('.modal-title').textContent = 'Chỉnh sửa bài viết';
              postModal.classList.add('active');
          } else {
              // View post details
              // In a real application, you would navigate to the post details page
              // or show a different modal with full post content
              alert(`Xem bài viết: ${title}`);
          }
      });
  });
  
  // Handle approve and reject buttons
  document.querySelectorAll('.approve-btn, .reject-btn').forEach(button => {
      button.addEventListener('click', (e) => {
          e.preventDefault();
          const isApprove = button.classList.contains('approve-btn');
          const row = button.closest('tr');
          const statusCell = row.querySelector('td:nth-child(7) .badge');
          
          if (isApprove) {
              statusCell.className = 'badge green';
              statusCell.textContent = 'Đã duyệt';
              row.dataset.postStatus = 'approved';
          } else {
              statusCell.className = 'badge red';
              statusCell.textContent = 'Từ chối';
              row.dataset.postStatus = 'rejected';
          }
          
          // Hide the approve/reject buttons
          row.querySelector('.approve-btn').style.display = 'none';
          row.querySelector('.reject-btn').style.display = 'none';
      });
  });
  
  // Filter Functions
  function applyFilters() {
      const searchTerm = searchInput.value.toLowerCase();
      const postType = postTypeFilter.value;
      const postStatus = postStatusFilter.value;
      const postLocation = postLocationFilter.value;
      
      filteredRows = [...dataRows].filter(row => {
          // Filter by search term
          const titleElement = row.querySelector('.table-title-cell');
          const contentElement = row.querySelector('.table-content-preview');
          const title = titleElement ? titleElement.textContent.toLowerCase() : '';
          const content = contentElement ? contentElement.textContent.toLowerCase() : '';
          const matchesSearch = searchTerm === '' || title.includes(searchTerm) || content.includes(searchTerm);
          
          // Filter by post type
          const rowPostType = row.dataset.postType;
          const matchesType = postType === 'all' || rowPostType === postType;
          
          // Filter by post status
          const rowPostStatus = row.dataset.postStatus;
          const matchesStatus = postStatus === 'all' || rowPostStatus === postStatus;
          
          // Filter by location
          const rowPostLocation = row.dataset.postLocation;
          const matchesLocation = postLocation === 'all' || rowPostLocation === postLocation;
          
          return matchesSearch && matchesType && matchesStatus && matchesLocation;
      });
      
      // Reset to first page when filters change
      currentPage = 1;
      updatePagination();
  }
  
  // Pagination Functions
  function updatePagination() {
      // Update total items count
      totalItemsElement.textContent = filteredRows.length;
      
      // Calculate total pages
      totalPages = Math.ceil(filteredRows.length / itemsPerPage);
      if (totalPages === 0) totalPages = 1;
      
      // Update page display
      currentPageElement.textContent = currentPage;
      totalPagesElement.textContent = totalPages;
      
      // Enable/disable pagination buttons
      prevPageButton.disabled = currentPage <= 1;
      nextPageButton.disabled = currentPage >= totalPages;
      
      // Show appropriate rows for current page
      const startIndex = (currentPage - 1) * itemsPerPage;
      const endIndex = startIndex + itemsPerPage;
      
      // First hide all rows
      dataRows.forEach(row => {
          row.classList.add('filtered');
          row.style.display = 'none';
      });
      
      // Then show only filtered rows for current page
      filteredRows.forEach((row, index) => {
          if (index >= startIndex && index < endIndex) {
              row.classList.remove('filtered');
              row.style.display = '';
          }
      });
  }
  
  function goToPrevPage() {
      if (currentPage > 1) {
          currentPage--;
          updatePagination();
      }
  }
  
  function goToNextPage() {
      if (currentPage < totalPages) {
          currentPage++;
          updatePagination();
      }
  }
  
  function changeItemsPerPage() {
      itemsPerPage = parseInt(itemsPerPageSelect.value);
      currentPage = 1;  // Reset to first page
      updatePagination();
  }
  
  function openPostModal() {
      // Reset form
      document.getElementById('postForm').reset();
      document.querySelector('.modal-title').textContent = 'Thêm bài viết mới';
      postModal.classList.add('active');
  }
  
  // Handle form submission
  document.getElementById('postForm').addEventListener('submit', function(e) {
      e.preventDefault();
      
      // In a real application, you would send form data to the server
      // and then update the UI after successful response
      
      const formData = {
          title: document.getElementById('title').value,
          postType: document.getElementById('post-type').value,
          location: document.getElementById('location').value,
          author: document.getElementById('author').value,
          excerpt: document.getElementById('excerpt').value,
          content: document.getElementById('content').value
      };
      
      // Here you would typically call an API endpoint
      console.log('Form submitted:', formData);
      
      // Close the modal
      postModal.classList.remove('active');
      
      // Show success message
      alert('Bài viết đã được lưu thành công!');
      
      // In a real application, you would refresh the data from the server
      // or add the new post to the table
  });
  
  // Custom method to make selector more jQuery-like
  Element.prototype.contains = function(text) {
      return this.textContent.includes(text);
  };
});