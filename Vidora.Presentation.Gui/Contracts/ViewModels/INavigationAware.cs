using System.Threading.Tasks;

namespace Vidora.Presentation.Gui.Contracts.ViewModels;

public interface INavigationAware
{
    Task OnNavigatedToAsync(object parameter);

    Task OnNavigatedFromAsync();
}
