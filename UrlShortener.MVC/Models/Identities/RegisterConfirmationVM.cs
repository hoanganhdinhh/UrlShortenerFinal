using Microsoft.AspNetCore.Mvc;

namespace UrlShortener.MVC.Models.Identities
{
    [Bind("Email, UserId, Code, ReturnUrl")]
    public class RegisterConfirmationVM
    {
        public string Email { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public bool DisplayConfirmAccountLink { get; set; }
        public string EmailConfirmationUrl { get; set; } = string.Empty;
        public string ReturnUrl { get; set; } = "~/";

    }
}
