using Microsoft.EntityFrameworkCore;
using TourismWeb.Models;

namespace TourismWeb.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<TouristSpot> TouristSpots { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<PostImage> PostImages { get; set; }
        public DbSet<PostComment> PostComments { get; set; }
        public DbSet<SpotFavorite> SpotFavorites { get; set; }
        public DbSet<PostFavorite> PostFavorites { get; set; }
        public DbSet<SpotShare> SpotShares { get; set; }
        public DbSet<PostShare> PostShares { get; set; }
        public DbSet<SpotImage> SpotImages { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<SpotTag> SpotTags { get; set; }
        public DbSet<PostTag> PostTags { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Composite keys
            modelBuilder.Entity<SpotTag>()
                .HasKey(st => new { st.SpotId, st.TagId });

            modelBuilder.Entity<PostTag>()
                .HasKey(pt => new { pt.PostId, pt.TagId });

            // Quan hệ User
            modelBuilder.Entity<User>()
                .Property(u => u.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasDefaultValue("User");

            modelBuilder.Entity<User>()
                .Property(u => u.AvatarUrl)
                .HasDefaultValue("default-avatar.png");

            modelBuilder.Entity<User>()
                .Property(u => u.PhoneNumber)
                .HasDefaultValue("0000000000");

            // Quan hệ TouristSpot
            modelBuilder.Entity<TouristSpot>()
                .Property(ts => ts.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            // modelBuilder.Entity<TouristSpot>()
            //     .HasOne(ts => ts.Category)
            //     .WithMany(c => c.TouristSpots)
            //     .HasForeignKey(ts => ts.CategoryId)
            //     .OnDelete(DeleteBehavior.Restrict);

            // Quan hệ Post
            modelBuilder.Entity<Post>()
                .Property(p => p.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<Post>()
                .HasOne(p => p.User)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Tránh cascade từ User đến Post

            modelBuilder.Entity<Post>()
                .HasOne(p => p.Spot)
                .WithMany(s => s.Posts)
                .HasForeignKey(p => p.SpotId)
                .OnDelete(DeleteBehavior.Cascade); // Xóa Post khi Spot bị xóa

            // Quan hệ Review
            modelBuilder.Entity<Review>()
                .Property(r => r.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Spot)
                .WithMany(s => s.Reviews)
                .HasForeignKey(r => r.SpotId)
                .OnDelete(DeleteBehavior.Cascade);

            // Quan hệ PostImage
            modelBuilder.Entity<PostImage>()
                .HasOne(pi => pi.Post)
                .WithMany(p => p.Images)
                .HasForeignKey(pi => pi.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            // Quan hệ PostComment
            modelBuilder.Entity<PostComment>()
                .Property(pc => pc.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<PostComment>()
                .HasOne(pc => pc.User)
                .WithMany(u => u.PostComments)
                .HasForeignKey(pc => pc.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PostComment>()
                .HasOne(pc => pc.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(pc => pc.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            // Quan hệ SpotFavorite
            modelBuilder.Entity<SpotFavorite>()
                .Property(sf => sf.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<SpotFavorite>()
                .HasOne(sf => sf.User)
                .WithMany(u => u.SpotFavorites)
                .HasForeignKey(sf => sf.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SpotFavorite>()
                .HasOne(sf => sf.Spot)
                .WithMany(s => s.Favorites)
                .HasForeignKey(sf => sf.SpotId)
                .OnDelete(DeleteBehavior.Cascade);

            // Quan hệ PostFavorite
            modelBuilder.Entity<PostFavorite>()
        .HasKey(pf => pf.FavoriteId);
            modelBuilder.Entity<PostFavorite>()
                .Property(pf => pf.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<PostFavorite>()
        .HasOne(pf => pf.Post)
        .WithMany(p => p.PostFavorites)
        .HasForeignKey(pf => pf.PostId)
        .OnDelete(DeleteBehavior.Cascade); // Xóa PostFavorites khi Post bị xóa

    modelBuilder.Entity<PostFavorite>()
        .HasOne(pf => pf.User)
        .WithMany(u => u.PostFavorites)
        .HasForeignKey(pf => pf.UserId)
        .OnDelete(DeleteBehavior.NoAction); // Không xóa PostFavorites khi User bị xóa
// Quan hệ Post (đảm bảo Posts.UserId là CASCADE nếu cần)
    modelBuilder.Entity<Post>()
        .HasOne(p => p.User)
        .WithMany(u => u.Posts)
        .HasForeignKey(p => p.UserId)
        .OnDelete(DeleteBehavior.Cascade); // Xóa Posts khi User bị xóa
            // Quan hệ SpotShare
            modelBuilder.Entity<SpotShare>()
                .Property(ss => ss.SharedAt)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<SpotShare>()
                .HasOne(ss => ss.User)
                .WithMany(u => u.SpotShares)
                .HasForeignKey(ss => ss.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SpotShare>()
                .HasOne(ss => ss.Spot)
                .WithMany(s => s.Shares)
                .HasForeignKey(ss => ss.SpotId)
                .OnDelete(DeleteBehavior.Cascade);

            // Quan hệ PostShare
            modelBuilder.Entity<PostShare>()
                .Property(ps => ps.SharedAt)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<PostShare>()
                .HasOne(ps => ps.User)
                .WithMany(u => u.PostShares)
                .HasForeignKey(ps => ps.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PostShare>()
                .HasOne(ps => ps.Post)
                .WithMany(p => p.Shares)
                .HasForeignKey(ps => ps.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            // Quan hệ SpotImage
            modelBuilder.Entity<SpotImage>()
                .Property(si => si.UploadedAt)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<SpotImage>()
                .HasOne(si => si.Spot)
                .WithMany(s => s.Images)
                .HasForeignKey(si => si.SpotId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SpotImage>()
                .HasOne(si => si.User)
                .WithMany(u => u.SpotImages)
                .HasForeignKey(si => si.UploadedBy)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<PostImage>()
                .HasOne(si => si.User)
                .WithMany(u => u.PostImages)
                .HasForeignKey(si => si.UploadedBy)
                .OnDelete(DeleteBehavior.NoAction); // Không xóa PostImages khi User bị xóa

            // Quan hệ SpotTag
            modelBuilder.Entity<SpotTag>()
                .HasOne(st => st.Spot)
                .WithMany(s => s.SpotTags)
                .HasForeignKey(st => st.SpotId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SpotTag>()
                .HasOne(st => st.Tag)
                .WithMany(t => t.SpotTags)
                .HasForeignKey(st => st.TagId)
                .OnDelete(DeleteBehavior.Restrict);

            // Quan hệ PostTag
            modelBuilder.Entity<PostTag>()
                .HasOne(pt => pt.Post)
                .WithMany(p => p.PostTags)
                .HasForeignKey(pt => pt.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PostTag>()
                .HasOne(pt => pt.Tag)
                .WithMany(t => t.PostTags)
                .HasForeignKey(pt => pt.TagId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}