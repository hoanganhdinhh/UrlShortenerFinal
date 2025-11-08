using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace UrlShortener.MVC.Models.Identities
{
    [Bind("Email, ProviderDisplayName, ReturnUrl")]
    public class ExternalLoginVM
    {

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        public string ProviderDisplayName { get; set; } = string.Empty;
        public string ReturnUrl { get; set; } = "~/";


    }
}
