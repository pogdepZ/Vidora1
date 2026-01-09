using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using Vidora.Presentation.Gui.ViewModels;

namespace Vidora.Presentation.Gui.Views;

public sealed partial class ManageMoviesPage : Page
{
    public ManageMoviesViewModel ViewModel { get; } = App.GetService<ManageMoviesViewModel>();

    public ManageMoviesPage()
    {
        InitializeComponent();
        DataContext = ViewModel;
    }

    private async void OnSearchQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        => await ViewModel.SearchCommand.ExecuteAsync(null);
}
