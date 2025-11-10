using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;


namespace UrlShortener.Services.Otp;

public static class OtpServiceExtensions 
{
    public static IServiceCollection AddOtpService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMemoryCache();
        services.Configure<OtpOptions>(configuration.GetSection("Otp"));
        services.AddScoped<IOtpService, OtpService>();
        return services;
    }
}
