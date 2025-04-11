// Initialize the map
function initMap() {
    if (document.getElementById("map")) {
      // Check if Leaflet library is available
      if (typeof L === "undefined") {
        console.error("Leaflet library is not loaded. Please ensure it is included in your HTML.")
        return
      }
  
      const map = L.map("map").setView([16.0544, 108.2022], 13) // Centered on Da Nang
  
      // Add the OpenStreetMap tile layer
      L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors',
      }).addTo(map)
  
      // Add a marker for Da Nang
      L.marker([16.0544, 108.2022]).addTo(map).bindPopup("<b>Đà Nẵng</b><br>Thành phố biển xinh đẹp").openPopup()
    }
  }
  
  // Toggle favorite button
  function setupFavoriteButton() {
    const favoriteBtn = document.getElementById("favoriteBtn")
    if (favoriteBtn) {
      favoriteBtn.addEventListener("click", function () {
        this.classList.toggle("active")
        const icon = this.querySelector("i")
        if (this.classList.contains("active")) {
          icon.classList.remove("far")
          icon.classList.add("fas")
          this.innerHTML = '<i class="fas fa-heart"></i> Đã yêu thích'
        } else {
          icon.classList.remove("fas")
          icon.classList.add("far")
          this.innerHTML = '<i class="far fa-heart"></i> Yêu thích'
        }
      })
    }
  }
  
  // Gallery functionality
  function setupGallery() {
    const mainImage = document.querySelector(".main-image img")
    const thumbnails = document.querySelectorAll(".thumbnails img")
  
    if (mainImage && thumbnails.length) {
      thumbnails.forEach((thumbnail) => {
        thumbnail.addEventListener("click", function () {
          mainImage.src = this.src
          thumbnails.forEach((thumb) => thumb.classList.remove("active"))
          this.classList.add("active")
        })
      })
    }
  }
  
  // Lightbox functionality
  function setupLightbox() {
    // Create lightbox elements if they don't exist
    if (!document.querySelector(".lightbox")) {
      const lightbox = document.createElement("div")
      lightbox.className = "lightbox"
      lightbox.innerHTML = `
              <span class="close-lightbox">&times;</span>
              <img class="lightbox-content">
          `
      document.body.appendChild(lightbox)
  
      // Close lightbox when clicking on the X or outside the image
      const closeLightbox = document.querySelector(".close-lightbox")
      closeLightbox.addEventListener("click", () => {
        document.querySelector(".lightbox").style.display = "none"
      })
  
      lightbox.addEventListener("click", function (e) {
        if (e.target === this) {
          this.style.display = "none"
        }
      })
    }
  
    // Add click event to all images that should open in lightbox
    const galleryImages = document.querySelectorAll(".review-photos img, .comment-media img")
    galleryImages.forEach((img) => {
      img.addEventListener("click", function () {
        openLightbox(this.src)
      })
    })
  }
  
  // Open lightbox with specific image
  function openLightbox(src) {
    const lightbox = document.querySelector(".lightbox")
    const lightboxImg = document.querySelector(".lightbox-content")
  
    lightboxImg.src = src
    lightbox.style.display = "block"
  }
  
  // Load more reviews
  function setupLoadMore() {
    const loadMoreReviews = document.getElementById("loadMoreReviews")
    if (loadMoreReviews) {
      loadMoreReviews.addEventListener("click", function () {
        // This would typically load more reviews from the server
        // For demo purposes, we'll just show a message
        this.textContent = "Đang tải..."
        setTimeout(() => {
          this.textContent = "Không còn đánh giá nào khác"
          this.disabled = true
        }, 1500)
      })
    }
  
    const loadMoreComments = document.getElementById("loadMoreComments")
    if (loadMoreComments) {
      loadMoreComments.addEventListener("click", function () {
        // This would typically load more comments from the server
        // For demo purposes, we'll just show a message
        this.textContent = "Đang tải..."
        setTimeout(() => {
          this.textContent = "Không còn bình luận nào khác"
          this.disabled = true
        }, 1500)
      })
    }
  }
  
  // Filter reviews
  function setupFilters() {
    const filterButtons = document.querySelectorAll(".filter-btn")
    if (filterButtons.length) {
      filterButtons.forEach((button) => {
        button.addEventListener("click", function () {
          // Remove active class from all buttons
          filterButtons.forEach((btn) => btn.classList.remove("active"))
          // Add active class to clicked button
          this.classList.add("active")
  
          // This would typically filter the reviews based on the data-filter attribute
          // For demo purposes, we'll just log the filter
          console.log("Filter by:", this.dataset.filter)
        })
      })
    }
  }
  
  // Initialize all functionality when DOM is loaded
  document.addEventListener("DOMContentLoaded", () => {
    initMap()
    setupFavoriteButton()
    setupGallery()
    setupLightbox()
    setupLoadMore()
    setupFilters()
  
    // Add smooth scrolling for all anchor links
    document.querySelectorAll('a[href^="#"]').forEach((anchor) => {
      anchor.addEventListener("click", function (e) {
        e.preventDefault()
        const target = document.querySelector(this.getAttribute("href"))
        if (target) {
          target.scrollIntoView({
            behavior: "smooth",
          })
        }
      })
    })
  
    // Initialize comment posting
    const postCommentBtn = document.getElementById("postCommentBtn")
    const commentInput = document.getElementById("commentInput")
  
    if (postCommentBtn && commentInput) {
      postCommentBtn.addEventListener("click", () => {
        if (commentInput.value.trim() !== "") {
          alert("Bình luận của bạn đã được gửi và đang chờ phê duyệt!")
          commentInput.value = ""
        } else {
          alert("Vui lòng nhập nội dung bình luận!")
        }
      })
    }
  })
  