using Microsoft.UI.Xaml;
using System.Threading.Tasks;
using Vidora.Presentation.Gui.Activation;
using Vidora.Presentation.Gui.Contracts.Services;
using Vidora.Presentation.Gui.Views;

namespace Vidora.Presentation.Gui.Services;

public class ActivationService : IActivationService
{
    private readonly ActivationHandler<LaunchActivatedEventArgs> _defaultHandler;
    private readonly IThemeSelectorService _themeSelectorService;
    public ActivationService(
        ActivationHandler<LaunchActivatedEventArgs> defaultHandler,
        IThemeSelectorService themeSelectorService)
    {
        _defaultHandler = defaultHandler;
        _themeSelectorService = themeSelectorService;
    }

    public async Task ActivateAsync(object activationArgs)
    {
        // Execute tasks before activation.
        await InitializeAsync();

        // Set the MainWindow Content.
        if (App.MainWindow.Content == null)
            App.MainWindow.Content = App.GetService<ShellPage>();

        // Handle activation via ActivationHandlers.
        await HandleActivationAsync(activationArgs);

        // Activate the MainWindow
        App.MainWindow.Activate();

        // Execute tasks after activation.
        await StartupAsync();
    }

    private async Task HandleActivationAsync(object activationArgs)
    {
        if (_defaultHandler.CanHandle(activationArgs))
        {
            await _defaultHandler.HandleAsync(activationArgs);
        }
    }

    private async Task InitializeAsync()
    {
        _themeSelectorService.Initialize();
        await Task.CompletedTask;
    }

    private async Task StartupAsync()
    {
        _themeSelectorService.SetRequestedTheme();
        await Task.CompletedTask;
    }
}