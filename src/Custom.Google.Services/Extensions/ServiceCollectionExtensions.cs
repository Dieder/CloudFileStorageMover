using Custom.Google.Services.Options;
using Custom.Google.Services.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Custom.Google.Services.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGooglePhotoPickerServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<GoogleAuthOptions>(configuration.GetSection(GoogleAuthOptions.SectionName));

        var googleOptions = configuration.GetSection(GoogleAuthOptions.SectionName).Get<GoogleAuthOptions>()
            ?? throw new InvalidOperationException("Google auth configuration is missing.");

        var clientSecret = string.IsNullOrWhiteSpace(googleOptions.ClientSecret)
            ? googleOptions.SecretId
            : googleOptions.ClientSecret;

        services.AddAuthentication()
            .AddGoogle(options =>
            {
                options.ClientId = googleOptions.ClientId;
                options.ClientSecret = clientSecret;
                options.SaveTokens = true;
            });

        services.AddHttpClient<IGooglePhotosPickerService, GooglePhotosPickerService>();

        return services;
    }
}
