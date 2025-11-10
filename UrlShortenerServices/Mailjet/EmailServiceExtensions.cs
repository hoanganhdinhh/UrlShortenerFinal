using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace UrlShortener.Services.Mail.Mailjet;

public static class EmailServiceExtensions
{
    public static IServiceCollection AddMailjet(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MailjetOptions>(configuration.GetSection("Mailjet"));
        services.AddTransient<IEmailSender, MailjetEmailSender>();
        return services;
    }
}
