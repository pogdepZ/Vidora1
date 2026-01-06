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
        services.AddTransient<CheckHealthUseCase>();
        services.AddTransient<LoginUseCase>();
        services.AddTransient<AutoLoginUseCase>();
        services.AddTransient<LogoutUseCase>();
        services.AddTransient<RegisterUseCase>();

        // Services
        services.AddSingleton<ISessionStateService, SessionStateService>();
        services.AddTransient<IUserCredentialsService, UserCredentialsService>();

        // Mapping
        services.AddAutoMapper(typeof(LoginMappingProfile).Assembly);

        return services;
    }
}