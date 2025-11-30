using CommunityToolkit.Mvvm.ComponentModel;
using System.Threading.Tasks;
using Vidora.Presentation.Gui.Contracts.ViewModels;

namespace Vidora.Presentation.Gui.ViewModels;

public partial class ManageMoviesViewModel : ObservableRecipient, INavigationAware
{
    public async Task OnNavigatedToAsync(object parameter)
    {
    }

    public async Task OnNavigatedFromAsync()
    {
    }
}