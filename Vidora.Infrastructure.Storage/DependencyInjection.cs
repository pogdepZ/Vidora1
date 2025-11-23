using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Vidora.Infrastructure.Storage;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureStorage(this IServiceCollection services, IConfiguration configuration)
    {
        // TODO: Configure Infrastructure.Local services here

        return services;
    }
}
