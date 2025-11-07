using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using System.ComponentModel.DataAnnotations;
using UrlShortener.MVC.Commons;

namespace UrlShortener.MVC.Models
{
    [Bind("Id,OriginalUrl,ShortCode")]
    public class UrlVM
    {
        public int Id { get; set; }
        [MaxLength(MaxLengths.OriginalUrl)]
        public string OriginalUrl { get; set; } = string.Empty;
        [MaxLength(MaxLengths.ShortCode)]
        public string ShortCode { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public long ClickCount { get; set; }
        public string? UserId { get; set; }
    }
}
