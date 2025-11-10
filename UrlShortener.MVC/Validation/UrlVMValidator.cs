using FluentValidation;
using UrlShortener.MVC.Commons;
using UrlShortener.MVC.Models;

namespace UrlShortener.MVC.Validation
{
    public class UrlVMValidator : AbstractValidator<UrlVM>
    {
        public UrlVMValidator()
        {
            RuleFor(x => x.OriginalUrl)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Original URL is required.")
                .MaximumLength(MaxLengths.OriginalUrl).WithMessage($"Original URL must be at most {MaxLengths.OriginalUrl} characters.")
                .Must(BeAValidUrl).WithMessage("Original URL is not a valid URL.");

            RuleFor(x => x.ShortCode)
                .MaximumLength(MaxLengths.ShortCode).WithMessage($"Short code must be at most {MaxLengths.ShortCode} characters.")
                .Matches("^[A-Za-z0-9_-]*$").WithMessage("Short code may contain only letters, digits, dash and underscore.")
                .When(x => !string.IsNullOrWhiteSpace(x.ShortCode));
        }

        private static bool BeAValidUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return false;

            // Try parse as-is or with https scheme
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                if (!Uri.TryCreate("https://" + url, UriKind.Absolute, out uri))
                    return false;
            }

            // Must have http/https scheme
            if (!uri.Scheme.Equals("http", StringComparison.OrdinalIgnoreCase) &&
                !uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
                return false;

            var host = uri.Host ?? string.Empty;

            // Accept localhost
            if (string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase))
                return true;

            // Accept IP addresses
            var hostType = Uri.CheckHostName(host);
            if (hostType == UriHostNameType.IPv4 || hostType == UriHostNameType.IPv6)
                return true;

            // Require a dot in hostname for regular domain names (e.g. example.com)
            if (host.Contains('.'))
                return true;

            // Otherwise reject (too short/unknown host like "asdsd")
            return false;
        }
    }
}
