using Microsoft.EntityFrameworkCore;
using UrlShortener.MVC.Commons;
using UrlShortener.MVC.Data.Entities;
using UrlShortener.MVC.Data.Entities.Identities; // <— thêm dòng này

namespace UrlShortener.MVC.Data
{
    public class UrlShortenerDbContext : DbContext
    {
        public UrlShortenerDbContext(DbContextOptions<UrlShortenerDbContext> options)
            : base(options) { }

        public DbSet<Url> Urls => Set<Url>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Url>(entity =>
            {
                // Required + MaxLength
                entity.Property(u => u.OriginalUrl)
                      .IsRequired()
                      .HasMaxLength(MaxLengths.OriginalUrl);

                entity.Property(u => u.ShortCode)
                      .IsRequired()
                      .HasMaxLength(MaxLengths.ShortCode);

                // Ràng buộc FK tới bảng AspNetUsers (Identity)
                // UrlShortenerUser: khóa chính kiểu string
                entity.HasIndex(u => u.ShortCode).IsUnique();
                entity.HasOne(u => u.User).WithMany()
                      .HasForeignKey(u => u.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

            });
        }
    }
}
