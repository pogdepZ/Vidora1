using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vidora.Core.Interfaces.Storage;
using Vidora.Infrastructure.Storage.Services;

namespace Vidora.Infrastructure.Storage;

public static class DependencyInjection
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        // TODO: Configure Infrastructure.Storage services here
        services.AddSingleton<ILocalSettingsService, LocalSettingsService>();
        services.AddSingleton<ISecureVaultService, SecureVaultService>();
        services.AddSingleton<IIdentityVerificationService, IdentityVerificationService>();

        return services;
    }
}
