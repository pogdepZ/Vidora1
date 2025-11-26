using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vidora.Core.Interfaces.Storage;
using Vidora.Infrastructure.Platform.Services;

namespace Vidora.Infrastructure.Platform;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructurePlatform(this IServiceCollection services, IConfiguration configuration)
    {
        // TODO: Configure Infrastructure.Platform services here
        services.AddSingleton<ILocalSettingsService, LocalSettingsService>();
        services.AddSingleton<ISecureVaultService, SecureVaultService>();
        services.AddSingleton<ISessionStorageService, SessionStorageService>();

        return services;
    }
}
