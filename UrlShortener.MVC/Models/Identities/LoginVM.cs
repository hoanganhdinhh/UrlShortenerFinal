using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace UrlShortener.MVC.Models.Identities
{
    [Bind("Email, Password, RememberMe, ReturnUrl, ExternalLogins")]
    public class LoginVM
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; } = false;

        public string ReturnUrl { get; set; } = "~/";

        public IList<AuthenticationScheme>? ExternalLogins { get; set; }
    }
}
