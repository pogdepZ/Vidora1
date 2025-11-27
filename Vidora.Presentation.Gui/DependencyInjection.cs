using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

using Vidora.Presentation.Gui.Activation;
using Vidora.Presentation.Gui.Contracts.Services;
using Vidora.Presentation.Gui.Mapping;
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
        services.AddSingleton<INavigationViewService, NavigationViewService>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IPageService, PageService>();
        services.AddSingleton<IInfoBarService, InfoBarService>();

        // Views
        services.AddTransient<ShellPage>();
        services.AddTransient<SplashPage>();
        services.AddTransient<LoginPage>();
        services.AddTransient<RegisterPage>();
        services.AddTransient<SettingsPage>();


        // ViewModels
        services.AddTransient<ShellViewModel>();
        services.AddTransient<SplashViewModel>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<RegisterViewModel>();
        services.AddTransient<SettingsViewModel>();


        // Mapping
        services.AddAutoMapper(typeof(UserProfile).Assembly);


        // Config

        return services;
    }
}
