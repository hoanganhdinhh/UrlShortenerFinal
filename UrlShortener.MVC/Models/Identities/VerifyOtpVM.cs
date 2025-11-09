using System.ComponentModel.DataAnnotations;

namespace UrlShortener.MVC.Models.Identities
{
    public class VerifyOtpVM
    {
        [Required, EmailAddress] public string Email { get; set; } = string.Empty;
        [Required] public string Purpose { get; set; } = "confirm-email";
        [Required, StringLength(6, MinimumLength = 6)] public string Otp { get; set; } = string.Empty;
        public string ReturnUrl { get; set; } = "/";
    }
}
