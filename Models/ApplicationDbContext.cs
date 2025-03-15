using Microsoft.EntityFrameworkCore;

namespace TourismWeb.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<TouristSpot> TouristSpots { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<Share> Shares { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<Video> Videos { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Đặt giá trị mặc định cho CreatedAt
            modelBuilder.Entity<User>()
                .Property(u => u.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<TouristSpot>()
                .Property(ts => ts.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<Favorite>()
                .Property(f => f.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            // Sửa lỗi quan hệ của Favorite
            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.User)
                .WithMany(u => u.Favorites)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.NoAction); 

            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.TouristSpot)
                .WithMany(ts => ts.Favorites)
                .HasForeignKey(f => f.SpotId)
                .OnDelete(DeleteBehavior.Cascade); 

            // Review - User
            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Comment - User
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Share - User
            modelBuilder.Entity<Share>()
                .HasOne(s => s.User)
                .WithMany(u => u.Shares)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.SetNull);
            
            
            // Cấu hình kiểu decimal với độ chính xác 18,2
            modelBuilder.Entity<TouristSpot>()
                .Property(ts => ts.EntranceFee)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Image>()
                .HasOne(i => i.Uploader)
                .WithMany(u => u.Images)
                .HasForeignKey(i => i.UploadedBy)
                .OnDelete(DeleteBehavior.SetNull); 

            modelBuilder.Entity<Image>()
                .HasOne(i => i.TouristSpot)
                .WithMany(s => s.Images)
                .HasForeignKey(i => i.SpotId)
                .OnDelete(DeleteBehavior.Cascade); 

            base.OnModelCreating(modelBuilder);

        }
    }
}
