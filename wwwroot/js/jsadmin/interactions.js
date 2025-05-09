// document.addEventListener('DOMContentLoaded', function() {
//   // Hàm để cập nhật bảng top bài viết
//   function updateTopPostsTable(data) {
//       const tableBody = document.getElementById('top-posts-table');
//       tableBody.innerHTML = '';

//       if (!data || data.length === 0) {
//           const row = document.createElement('tr');
//           row.innerHTML = '<td colspan="7" class="text-center">Không có dữ liệu</td>';
//           tableBody.appendChild(row);
//           return;
//       }

//       data.forEach(post => {
//           const row = document.createElement('tr');

//           // Xác định loại badge dựa trên loại bài viết
//           let badgeClass = 'blue';
//           let badgeIcon = 'fa-map-marker-alt';
//           let badgeText = 'Địa điểm';

//           if (post.type === 'Cẩm nang') {
//               badgeClass = 'green';
//               badgeIcon = 'fa-book';
//               badgeText = 'Cẩm nang';
//           } else if (post.type === 'Trải nghiệm') {
//               badgeClass = 'amber';
//               badgeIcon = 'fa-star';
//               badgeText = 'Trải nghiệm';
//           }

//           const totalInteractions = post.viewCount + post.likeCount + post.saveCount + post.shareCount;

//           row.innerHTML = `
//               <td>
//                   <div class="table-title-cell">${post.title}</div>
//               </td>
//               <td>
//                   <span class="badge ${badgeClass}">
//                       <i class="fas ${badgeIcon}"></i>
//                       ${badgeText}
//                   </span>
//               </td>
//               <td>${post.viewCount}</td>
//               <td>${post.likeCount}</td>
//               <td>${post.saveCount}</td>
//               <td>${post.shareCount}</td>
//               <td>
//                   <span class="total-interactions">${totalInteractions}</span>
//               </td>
//           `;

//           tableBody.appendChild(row);
//       });
//   }

//   // Hàm để cập nhật bảng top địa điểm
//   function updateTopSpotsTable(data) {
//       const tableBody = document.getElementById('top-spots-table');
//       tableBody.innerHTML = '';

//       if (!data || data.length === 0) {
//           const row = document.createElement('tr');
//           row.innerHTML = '<td colspan="7" class="text-center">Không có dữ liệu</td>';
//           tableBody.appendChild(row);
//           return;
//       }

//       data.forEach(spot => {
//           const row = document.createElement('tr');

//           const totalInteractions = spot.viewCount + spot.likeCount + spot.saveCount + spot.shareCount;

//           row.innerHTML = `
//               <td>
//                   <div class="table-title-cell">${spot.name}</div>
//               </td>
//               <td>${spot.address}</td>
//               <td>${spot.viewCount}</td>
//               <td>${spot.likeCount}</td>
//               <td>${spot.saveCount}</td>
//               <td>${spot.shareCount}</td>
//               <td>
//                   <span class="total-interactions">${totalInteractions}</span>
//               </td>
//           `;

//           tableBody.appendChild(row);
//       });
//   }

//   // Hàm để cập nhật bảng top người dùng
//   function updateTopUsersTable(data) {
//       const tableBody = document.getElementById('top-users-table');
//       tableBody.innerHTML = '';

//       if (!data || data.length === 0) {
//           const row = document.createElement('tr');
//           row.innerHTML = '<td colspan="7" class="text-center">Không có dữ liệu</td>';
//           tableBody.appendChild(row);
//           return;
//       }

//       data.forEach(user => {
//           const row = document.createElement('tr');

//           const totalInteractions = user.viewCount + user.likeCount + user.saveCount + user.shareCount + user.commentCount;

//           row.innerHTML = `
//               <td>
//                   <div class="user-cell">
//                       <div class="user-avatar">
//                           <img src="${user.avatarUrl || 'https://i.pravatar.cc/40?img=' + user.userId}" alt="Avatar">
//                       </div>
//                       <div class="user-info">
//                           <div class="user-name">${user.username}</div>
//                           <div class="user-email">${user.email}</div>
//                       </div>
//                   </div>
//               </td>
//               <td>${user.viewCount}</td>
//               <td>${user.likeCount}</td>
//               <td>${user.saveCount}</td>
//               <td>${user.shareCount}</td>
//               <td>${user.commentCount}</td>
//               <td>
//                   <span class="total-interactions">${totalInteractions}</span>
//               </td>
//           `;

//           tableBody.appendChild(row);
//       });
//   }

//   // Lấy dữ liệu cho các bảng khi trang tải xong
//   function loadTableData(days = 30) {
//       // Lấy dữ liệu cho bảng top bài viết
//       fetch(`/Admin/GetTopPosts?days=${days}`)
//           .then(response => response.json())
//           .then(data => updateTopPostsTable(data))
//           .catch(error => console.error('Error loading top posts:', error));

//       // Lấy dữ liệu cho bảng top địa điểm
//       fetch(`/Admin/GetTopSpots?days=${days}`)
//           .then(response => response.json())
//           .then(data => updateTopSpotsTable(data))
//           .catch(error => console.error('Error loading top spots:', error));

//       // Lấy dữ liệu cho bảng top người dùng
//       fetch(`/Admin/GetTopUsers?days=${days}`)
//           .then(response => response.json())
//           .then(data => updateTopUsersTable(data))
//           .catch(error => console.error('Error loading top users:', error));
//   }

//   // Khi thay đổi khoảng thời gian, cập nhật lại dữ liệu cho các bảng
//   document.getElementById('time-range').addEventListener('change', function() {
//       const days = this.value === 'all' ? 0 : parseInt(this.value);
//       loadTableData(days);
//   });

//   // Tải dữ liệu ban đầu
//   loadTableData();
// });
