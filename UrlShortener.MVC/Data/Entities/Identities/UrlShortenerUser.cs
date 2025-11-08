using Microsoft.AspNetCore.Identity;
using System.ComponentModel;
using UrlShortener.MVC.Commons.Enums;

namespace UrlShortener.MVC.Data.Entities.Identities
{
    public class UrlShortenerUser : IdentityUser
    {
        public string? FullName { get; set; }
        public DateTime? DateOfBirth { get; set; }

        public GenderEnum Gender { get; set; }

        public string? Address { get; set; }

        public string? AvatarUrl { get; set; }
        public string? Bio { get; set; }
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
