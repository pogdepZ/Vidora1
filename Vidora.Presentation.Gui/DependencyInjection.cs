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
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Activation
        services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

        // Services
        services.AddSingleton<IActivationService, ActivationService>();
        services.AddSingleton<INavigationViewService, NavigationViewService>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IPageService, PageService>();
        services.AddSingleton<IInfoBarService, InfoBarService>();
        services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();


        // Views
        services.AddTransient<ShellPage>();
        services.AddTransient<SplashPage>();
        services.AddTransient<LoginPage>();
        services.AddTransient<RegisterPage>();
        services.AddTransient<SettingsPage>();
        services.AddTransient<HomePage>();
        services.AddTransient<SearchPage>();
        services.AddTransient<WatchlistPage>();
        services.AddTransient<SubscriptionPage>();
        services.AddTransient<CheckoutPage>();
        services.AddTransient<ProfilePage>();
        services.AddTransient<MovieDetailPage>();
        services.AddTransient<VideoPlayerPage>();

        services.AddTransient<AdminDashboardPage>();
        services.AddTransient<AdminReportsPage>();
        services.AddTransient<ManageMoviesPage>();
        services.AddTransient<ManageSubscriptionsPage>();
        services.AddTransient<ManageUsersPage>();


        // ViewModels
        services.AddTransient<ShellViewModel>();
        services.AddTransient<SplashViewModel>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<RegisterViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<HomeViewModel>();
        services.AddTransient<SearchViewModel>();
        services.AddTransient<WatchlistViewModel>();
        services.AddTransient<SubscriptionViewModel>();
        services.AddTransient<CheckoutViewModel>();
        services.AddTransient<ProfileViewModel>();
        services.AddTransient<MovieDetailViewModel>();
        services.AddTransient<VideoPlayerViewModel>();

        services.AddTransient<AdminDashboardViewModel>();
        services.AddTransient<AdminReportsViewModel>();
        services.AddTransient<ManageMoviesViewModel>();
        services.AddTransient<ManageSubscriptionsViewModel>();
        services.AddTransient<ManageUsersViewModel>();

        services.AddTransient<HeroSectionItemViewModel>(); // Controls are only allowed to register viewmodels


        // Mapping
        services.AddAutoMapper(typeof(UserProfile).Assembly);


        // Config

        return services;
    }
}
