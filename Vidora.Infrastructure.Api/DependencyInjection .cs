using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Vidora.Infrastructure.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureApi(this IServiceCollection services, IConfiguration configuration)
    {
        // TODO: Configure Infrastructure.Api services here

        return services;
    }
}
