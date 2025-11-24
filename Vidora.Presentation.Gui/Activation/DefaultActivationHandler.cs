using Microsoft.UI.Xaml;
using System.Threading.Tasks;
using Vidora.Presentation.Gui.Contracts.Services;
using Vidora.Presentation.Gui.ViewModels;

namespace Vidora.Presentation.Gui.Activation;

public class DefaultActivationHandler : ActivationHandler<LaunchActivatedEventArgs>
{
    private readonly INavigationService _navigationService;
    public DefaultActivationHandler(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    protected override bool CanHandleInternal(LaunchActivatedEventArgs args)
    {
        return _navigationService.Frame?.Content == null;
    }

    protected async override Task HandleInternalAsync(LaunchActivatedEventArgs args)
    {
        await _navigationService.NavigateToAsync<SplashViewModel>(args.Arguments);
    }
}