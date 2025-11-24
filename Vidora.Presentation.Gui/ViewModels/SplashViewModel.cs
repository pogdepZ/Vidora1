using CommunityToolkit.Mvvm.ComponentModel;
using System.Threading.Tasks;
using Vidora.Presentation.Gui.Contracts.ViewModels;

namespace Vidora.Presentation.Gui.ViewModels;

public class SplashViewModel : ObservableRecipient, INavigationAware
{
    public SplashViewModel()
    {
    }

    public async Task OnNavigatedToAsync(object parameter)
    {
    }

    public async Task OnNavigatedFromAsync()
    {
    }
}
