using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vidora.Core.Interfaces.Api;
using Vidora.Infrastructure.Api.Services;

namespace Vidora.Infrastructure.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureApi(this IServiceCollection services, IConfiguration configuration)
    {
        // TODO: Configure Infrastructure.Api services here
        // Services
        services.AddTransient<IAuthApiService, AuthApiService>();


        return services;
    }
}
