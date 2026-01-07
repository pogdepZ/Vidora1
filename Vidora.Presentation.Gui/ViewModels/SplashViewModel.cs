using CommunityToolkit.Mvvm.ComponentModel;
using System.Threading.Tasks;
using Vidora.Core.Exceptions;
using Vidora.Core.UseCases;
using Vidora.Presentation.Gui.Contracts.Services;
using Vidora.Presentation.Gui.Contracts.ViewModels;

namespace Vidora.Presentation.Gui.ViewModels;

public partial class SplashViewModel : ObservableRecipient, INavigationAware
{
    private readonly AutoLoginUseCase _autoLoginUseCase;
    private readonly CheckHealthUseCase _checkHealthUseCase;
    private readonly IInfoBarService _infoBarService;
    public SplashViewModel(
        AutoLoginUseCase autoLoginUseCase,
        CheckHealthUseCase checkHealthUseCase,
        IInfoBarService infoBarService)
    {
        _autoLoginUseCase = autoLoginUseCase;
        _checkHealthUseCase = checkHealthUseCase;
        _infoBarService = infoBarService;
    }

    public async Task OnNavigatedToAsync(object parameter)
    {
        await Task.Delay(2000);
        try
        {
            await _checkHealthUseCase.ExecuteAsync();
        }
        catch (DomainException ex)
        {
            await _infoBarService.ShowErrorAsync(ex.Message);
        }

        try
        {
            await _autoLoginUseCase.ExecuteAsync();
        }
        catch (DomainException)
        {
        }
    }

    public async Task OnNavigatedFromAsync()
    {
    }
}
