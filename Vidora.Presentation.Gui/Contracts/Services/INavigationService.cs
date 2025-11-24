using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Threading.Tasks;

namespace Vidora.Presentation.Gui.Contracts.Services;


public interface INavigationService
{
    Frame? Frame { get; set; }

    bool CanGoBack { get; }

    Task<bool> NavigateToAsync<TViewModel>(object? parameter = null, bool clearNavigation = false)
        where TViewModel : class;

    Task<bool> NavigateToAsync(string pageKey, object? parameter = null, bool clearNavigation = false);

    Task<bool> GoBackAsync();

    event NavigatedEventHandler Navigated;
}