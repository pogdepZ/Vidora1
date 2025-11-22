using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

using Vidora.Presentation.Gui.Activation;
using Vidora.Presentation.Gui.Contracts.Services;
using Vidora.Presentation.Gui.Services;
using Vidora.Presentation.Gui.ViewModels;
using Vidora.Presentation.Gui.Views;


namespace Vidora.Presentation.Gui;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
    {
        // TODO: Configure Presentation.Gui services here

        // Activation
        services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

        // Services
        services.AddSingleton<IActivationService, ActivationService>();

        // Views
        services.AddTransient<ShellPage>();
        services.AddTransient<SplashPage>();

        // ViewModels
        services.AddTransient<ShellViewModel>();
        services.AddTransient<SplashViewModel>();

        // Config

        return services;
    }
}
