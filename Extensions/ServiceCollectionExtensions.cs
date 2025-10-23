using System.Net.Http.Headers;
using UABackbone_Backend.Interfaces;
using UABackbone_Backend.Services;

namespace UABackbone_Backend.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEmailService(this IServiceCollection services)
    {
        services.AddHttpClient<IEmailService, EmailService>(client =>
        {
            client.BaseAddress = new Uri("https://api.mailersend.com/v1/");
            
            var token = Environment.GetEnvironmentVariable("MAILER_SEND_API_KEY")
                        ?? throw new InvalidOperationException("Missing MAILER_SEND_API_KEY");

            client.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", token);
        });
        
        return services;
    }

    public static IServiceCollection AddIdentityService(this IServiceCollection services)
    {
        services.AddScoped<IIdentityService, IdentityService>();
        return services;
    }
}