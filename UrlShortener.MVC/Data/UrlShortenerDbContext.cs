using Microsoft.EntityFrameworkCore;
using UrlShortener.MVC.Commons;
using UrlShortener.MVC.Data.Entities;

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
                entity.Property(u => u.OriginalUrl)
                      .HasMaxLength(MaxLengths.OriginalUrl);
                entity.Property(u => u.ShortCode)
                      .HasMaxLength(MaxLengths.ShortCode);
            });
        }
    }
}
