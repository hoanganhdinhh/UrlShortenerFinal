using System.ComponentModel.DataAnnotations;
using UrlShortener.MVC.Commons;
using UrlShortener.MVC.Data.Entities.Identities;

namespace UrlShortener.MVC.Data.Entities
{
    public class Url
    {
        //=== Properties ===//
        #region Properties
        public int Id { get; set; }
        [MaxLength(MaxLengths.OriginalUrl)]
        public string OriginalUrl { get; set; } = string.Empty;

        [MaxLength(MaxLengths.ShortCode)]

        public string ShortCode { get; set; } = string.Empty;

        public long ClickCount { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? UserId { get; set; } = string.Empty;  // khóa ngoại nếu dùng Microsoft Identity

        public UrlShortenerUser? User { get; set; } // Navigation property
        #endregion
    }
}
