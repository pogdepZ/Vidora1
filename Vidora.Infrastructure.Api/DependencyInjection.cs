using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vidora.Core.Interfaces.Api;
using Vidora.Infrastructure.Api.Mapping;
using Vidora.Infrastructure.Api.Options;
using Vidora.Infrastructure.Api.Services;

namespace Vidora.Infrastructure.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        // TODO: Configure Infrastructure.Api services here
        // Services
        services.AddTransient<IHealthApiService, HealthApiService>();
        services.AddTransient<IAuthApiService, AuthApiService>();


        //
        services.AddSingleton<ApiClient>();

        //
        services.AddAutoMapper(typeof(AuthMappingProfile).Assembly);

        services.Configure<ApiOptions>(configuration.GetSection(ApiOptions.SectionName));
        return services;
    }
}
