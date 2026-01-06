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
        // UseCases
        services.AddTransient<LoginUseCase>();
        services.AddTransient<AutoLoginUseCase>();
        services.AddTransient<LogoutUseCase>();
        services.AddTransient<RegisterUseCase>();
        services.AddTransient<GetProfileUseCase>();
        services.AddTransient<UpdateProfileUseCase>();
        services.AddTransient<SearchMovieUseCase>();
        services.AddTransient<GetMovieDetailUseCase>();
        services.AddTransient<GetGenresUseCase>();
        services.AddTransient<GetSubscriptionPlansUseCase>();
        services.AddTransient<GetMySubscriptionsUseCase>();
        services.AddTransient<CreateOrderUseCase>();
        services.AddTransient<GetAvailablePromosUseCase>();
        services.AddTransient<ApplyDiscountUseCase>();
        services.AddTransient<ConfirmPaymentUseCase>();

        // Services
        services.AddSingleton<ISessionStateService, SessionStateService>();
        services.AddTransient<IUserCredentialsService, UserCredentialsService>();

        // Mapping
        services.AddAutoMapper(typeof(LoginMappingProfile).Assembly);

        return services;
    }
}