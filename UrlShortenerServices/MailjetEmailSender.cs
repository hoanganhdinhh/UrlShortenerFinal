using Mailjet.Client;
using Mailjet.Client.TransactionalEmails;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using System.Linq;


namespace UrlShortener.Services;

public class MailjetEmailSender : IEmailSender
{
    private readonly MailjetClient _client;
    private readonly MailjetOptions _options;

    public MailjetEmailSender(IOptions<MailjetOptions> options)
    {
        _options = options.Value;
        _client = new MailjetClient(_options.ApiKey, _options.ApiSecret); // ❌ KHÔNG set Version
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var from = (_options.FromEmail ?? "").Trim();
        if (string.IsNullOrWhiteSpace(from) || !from.Contains("@"))
            throw new InvalidOperationException("Mailjet FromEmail is missing/invalid. Check appsettings + verify sender in Mailjet.");

        var to = (email ?? "").Trim();
        if (string.IsNullOrWhiteSpace(to) || !to.Contains("@"))
            throw new ArgumentException("Destination email is invalid.", nameof(email));

        var message = new TransactionalEmailBuilder()
            .WithFrom(new SendContact(from, _options.FromName))
            .WithTo(new SendContact(to))
            .WithSubject(subject)
            .WithHtmlPart(htmlMessage)
            .WithTextPart(StripHtml(htmlMessage))
            .Build();

        var response = await _client.SendTransactionalEmailAsync(message);

        if (response.Messages == null || !response.Messages.Any()
            || response.Messages.Any(m => !string.Equals(m.Status, "success", StringComparison.OrdinalIgnoreCase)))
        {
            var status = string.Join(", ", response.Messages?.Select(m => m.Status) ?? new[] { "no-messages" });
            throw new Exception($"Mailjet send failed: {status}");
        }
    }

    private static string StripHtml(string html) =>
        System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", string.Empty);
}
