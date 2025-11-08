using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UrlShortener.MVC.Commons;
using UrlShortener.MVC.Data.Entities.Identities;

namespace UrlShortener.MVC.Data
{
    public class UrlShortenerIdentityDbContext : IdentityDbContext<UrlShortenerUser>
    {
        public UrlShortenerIdentityDbContext(DbContextOptions<UrlShortenerIdentityDbContext> options)
            : base(options)
        {
        }

        public DbSet<UrlShortenerUser> UrlShortenerUsers { get; set; }
        public DbSet<UrlShortenerRole> UrlShortenerRoles { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Cấu hình User
            builder.Entity<UrlShortenerUser>(entity =>
            {
                entity.Property(u => u.FullName)
                      .HasMaxLength(MaxLengths.FullName);

                entity.Property(u => u.Address)
                      .HasMaxLength(MaxLengths.Address);

                entity.Property(u => u.AvatarUrl)
                      .HasMaxLength(MaxLengths.FileName);

                entity.Property(u => u.Bio)
                      .HasMaxLength(MaxLengths.Description);
            });

            // Cấu hình Role
            builder.Entity<UrlShortenerRole>(entity =>
            {
                entity.Property(r => r.Description)
                      .HasMaxLength(MaxLengths.Description);

                entity.Property(r => r.CreatedBy)
                      .HasMaxLength(MaxLengths.User);
            });
        }
    }
}
