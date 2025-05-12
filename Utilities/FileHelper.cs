namespace TourismWeb.Utilities
{
    public static class FileHelper
    {
        public static async Task<string> SaveFileAsync(IFormFile file, string folderPath)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File không hợp lệ.");
            }

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // Tạo tên file duy nhất để tránh trùng lặp
            var fileExtension = Path.GetExtension(file.FileName);
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(folderPath, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Trả về đường dẫn tương đối để lưu vào DB (ví dụ: /uploads/posts/guid.jpg)
            // Giả định folderPath là đường dẫn vật lý trong wwwroot
            // Cần lấy đường dẫn tương đối từ wwwroot
            string webRootPath = Path.DirectorySeparatorChar.ToString(); // Lấy dấu / hoặc \
            string relativePath = filePath.Substring(filePath.IndexOf(webRootPath + "wwwroot" + webRootPath) + ($"{webRootPath}wwwroot{webRootPath}").Length -1).Replace(Path.DirectorySeparatorChar, '/');


            return relativePath; // ví dụ trả về /uploads/posts/filename.jpg
             // Quan trọng: Đảm bảo đường dẫn trả về đúng định dạng bạn muốn lưu trong DB
             // ví dụ: nếu folderPath là C:\project\wwwroot\uploads\posts
             // và file là C:\project\wwwroot\uploads\posts\abc.jpg
             // bạn muốn trả về /uploads/posts/abc.jpg
        }
    }
}