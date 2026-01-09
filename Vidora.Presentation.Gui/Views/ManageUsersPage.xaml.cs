using Microsoft.UI.Xaml.Controls;
using Vidora.Presentation.Gui.ViewModels;

namespace Vidora.Presentation.Gui.Views;

public sealed partial class ManageUsersPage : Page
{
    public ManageUsersViewModel ViewModel { get; } = App.GetService<ManageUsersViewModel>();

    public ManageUsersPage()
    {
        InitializeComponent();
        DataContext = ViewModel;
    }

    private async void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        => await ViewModel.SearchCommand.ExecuteAsync(null);
}
