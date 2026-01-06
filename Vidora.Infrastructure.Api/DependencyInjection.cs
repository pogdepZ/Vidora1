using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vidora.Core.Interfaces.Api;
using Vidora.Core.Interfaces.Services;
using Vidora.Infrastructure.Api.Configuration;
using Vidora.Infrastructure.Api.Mapping;
using Vidora.Infrastructure.Api.Options;
using Vidora.Infrastructure.Api.Services;

namespace Vidora.Infrastructure.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureApi(this IServiceCollection services, IConfiguration configuration)
    {
        // Services
        services.AddTransient<IAuthApiService, AuthApiService>();
        services.AddTransient<IMovieApiService, MovieApiService>();
        services.AddTransient<ISubscriptionApiService, SubscriptionApiService>();
        services.AddTransient<IOrderApiService, OrderApiService>();
        services.AddTransient<IWatchlistApiService, WatchlistApiService>();

        // Cloudinary
        services.Configure<CloudinarySettings>(configuration.GetSection(CloudinarySettings.SectionName));
        services.AddSingleton<ICloudinaryService, CloudinaryService>();

        // API Client
        services.AddSingleton<ApiClient>();

        // AutoMapper
        services.AddAutoMapper(typeof(AuthMappingProfile).Assembly);

        // Options
        services.Configure<ApiOptions>(configuration.GetSection(ApiOptions.SectionName));

        return services;
    }
}
