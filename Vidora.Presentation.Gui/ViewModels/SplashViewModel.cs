using CommunityToolkit.Mvvm.ComponentModel;
using System.Threading.Tasks;
using Vidora.Core.UseCases;
using Vidora.Presentation.Gui.Contracts.Services;
using Vidora.Presentation.Gui.Contracts.ViewModels;

namespace Vidora.Presentation.Gui.ViewModels;

public class SplashViewModel : ObservableRecipient, INavigationAware
{
    private readonly INavigationService _navigationService;
    public SplashViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    public async Task OnNavigatedToAsync(object parameter)
    {
        await Task.Delay(2000);
        App.MainWindow.DispatcherQueue.TryEnqueue(async () =>
        {
            var uc = App.GetService<AutoLoginUseCase>();
            var result = await uc.ExecuteAsync();

            if (result.IsFailure)
            {
                await _navigationService.NavigateToAsync<LoginViewModel>(clearNavigation: true);
            }
        });
    }

    public async Task OnNavigatedFromAsync()
    {
    }
}
