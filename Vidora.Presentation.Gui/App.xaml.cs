using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using System;
using Vidora.Presentation.Gui.Contracts.Services;

namespace Vidora.Presentation.Gui;

public partial class App : Application
{
    public static Window MainWindow { get; } = new MainWindow();
    public IHost AppHost { get; }

    public App()
    {
        InitializeComponent();

        this.AppHost = Host
            .CreateDefaultBuilder()
            .UseContentRoot(AppContext.BaseDirectory)
            .ConfigureServices((context, services) =>
            {
                Core.DependencyInjection
                    .AddServices(services, context.Configuration);

                Infrastructure.Api.DependencyInjection
                    .AddServices(services, context.Configuration);

                Infrastructure.Storage.DependencyInjection
                    .AddServices(services, context.Configuration);

                Infrastructure.Persistence.DependencyInjection
                    .AddServices(services, context.Configuration);

                Presentation.Gui.DependencyInjection
                    .AddServices(services, context.Configuration);

            })
            .Build();
    }


    public static T GetService<T>() where T : class
    {
        if ((App.Current as App)!.AppHost.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return service;
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        await App.GetService<IActivationService>().ActivateAsync(args);
    }
}
