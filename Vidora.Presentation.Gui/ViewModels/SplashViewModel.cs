using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI;
using System.Threading.Tasks;
using Vidora.Core.UseCases;
using Vidora.Presentation.Gui.Contracts.Services;
using Vidora.Presentation.Gui.Contracts.ViewModels;

namespace Vidora.Presentation.Gui.ViewModels;

public partial class SplashViewModel : ObservableRecipient, INavigationAware
{
    private readonly AutoLoginUseCase _autoLoginUseCase;
    private readonly CheckHealthUseCase _checkHealthUseCase;
    private readonly IInfoBarService _infoBarService;
    private readonly INavigationService _navigationService;
    public SplashViewModel(
        AutoLoginUseCase autoLoginUseCase,
        CheckHealthUseCase checkHealthUseCase,
        IInfoBarService infoBarService,
        INavigationService navigationService)
    {
        _autoLoginUseCase = autoLoginUseCase;
        _checkHealthUseCase = checkHealthUseCase;
        _infoBarService = infoBarService;
        _navigationService = navigationService;
    }

    public async Task OnNavigatedToAsync(object parameter)
    {
        await Task.Delay(2000);
        //await App.MainWindow.DispatcherQueue.EnqueueAsync(async () =>
        //{
        //    var uc = App.GetService<AutoLoginUseCase>();
        //    var result = await uc.ExecuteAsync();
        //});

        var healthResult = await _checkHealthUseCase.ExecuteAsync();
        if (healthResult.IsFailure)
        {
            await _infoBarService.ShowErrorAsync(healthResult.Error);
            // TODO
            await _navigationService.NavigateToAsync<LoginViewModel>(clearNavigation: true);
            return;
        }

        var autoLoginResult = await _autoLoginUseCase.ExecuteAsync();
        if (autoLoginResult.IsFailure)
        {
            await _navigationService.NavigateToAsync<LoginViewModel>(clearNavigation: true);
        }
    }

    public async Task OnNavigatedFromAsync()
    {
    }
}
