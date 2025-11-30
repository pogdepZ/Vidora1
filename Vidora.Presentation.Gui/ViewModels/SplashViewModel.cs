using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI;
using System.Threading.Tasks;
using Vidora.Core.UseCases;
using Vidora.Presentation.Gui.Contracts.ViewModels;

namespace Vidora.Presentation.Gui.ViewModels;

public partial class SplashViewModel : ObservableRecipient, INavigationAware
{
    public SplashViewModel()
    {
    }

    public async Task OnNavigatedToAsync(object parameter)
    {
        await Task.Delay(2000);
        await App.MainWindow.DispatcherQueue.EnqueueAsync(async () =>
        {
            var uc = App.GetService<AutoLoginUseCase>();
            var result = await uc.ExecuteAsync();
        });
    }

    public async Task OnNavigatedFromAsync()
    {
    }
}
