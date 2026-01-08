using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using Vidora.Core.Interfaces.Api;
using Vidora.Core.Interfaces.Storage;
using Vidora.Infrastructure.Api.Clients;
using Vidora.Infrastructure.Api.Handlers;
using Vidora.Infrastructure.Api.Mapping;
using Vidora.Infrastructure.Api.Options;
using Vidora.Infrastructure.Api.Services;

namespace Vidora.Infrastructure.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Services
        services.AddTransient<AuthTokenHandler>();
        services.AddHttpClient<ApiClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<ApiOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.Timeout);
        })
        .AddHttpMessageHandler<AuthTokenHandler>();

        services.AddTransient<IHealthApiService, HealthApiService>();
        services.AddTransient<IAuthApiService, AuthApiService>();
        services.AddTransient<IMovieApiService, MovieApiService>();
        services.AddTransient<ISubscriptionApiService, SubscriptionApiService>();
        services.AddTransient<IOrderApiService, OrderApiService>();
        services.AddTransient<IWatchlistApiService, WatchlistApiService>();
        services.AddTransient<IStatsApiService, StatsApiService>();


        // AutoMapper
        services.AddAutoMapper(typeof(AuthMappingProfile).Assembly);
        services.AddAutoMapper(typeof(MovieMappingProfile).Assembly);
        services.AddAutoMapper(typeof(OrderMappingProfile).Assembly);
        services.AddAutoMapper(typeof(SubscriptionMappingProfile).Assembly);
        services.AddAutoMapper(typeof(UserMappingProfile).Assembly);


        // Options
        services.Configure<ApiOptions>(configuration.GetSection(ApiOptions.SectionName));

        return services;
    }
}
