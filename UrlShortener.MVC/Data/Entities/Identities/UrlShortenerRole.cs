using Microsoft.AspNetCore.Identity;
using System.ComponentModel;

namespace UrlShortener.MVC.Data.Entities.Identities
{
    public class UrlShortenerRole : IdentityRole
    {
        [Description("Mô tả vai trò")]
        public string? Description { get; set; }

        [Description("Vai trò mặc định của hệ thống")]
        public bool IsSystemRole { get; set; } = false;

        [Description("Tạo lúc nào")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Description("Ai tạo")]
        public string? CreatedBy { get; set; }
    }
}
