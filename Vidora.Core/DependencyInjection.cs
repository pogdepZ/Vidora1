using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vidora.Core.Contracts.Services;
using Vidora.Core.Mapping;
using Vidora.Core.Services;
using Vidora.Core.UseCases;

namespace Vidora.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddCore(this IServiceCollection services, IConfiguration configuration)
    {
        // TODO: Configure Core services here
        services.AddTransient<LoginUseCase>();
        services.AddTransient<LogoutUseCase>();

        // Services
        services.AddSingleton<ISessionStateService, SessionStateService>();

        // Mapping
        services.AddAutoMapper(typeof(LoginProfile).Assembly);

        return services;
    }
}