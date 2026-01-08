using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Vidora.Presentation.Gui.ViewModels;

namespace Vidora.Presentation.Gui.Views;

public sealed partial class SearchPage : Page
{
    public SearchViewModel ViewModel { get; } = App.GetService<SearchViewModel>();

    public SearchPage()
    {
        InitializeComponent();
    }

    private void SearchBox_EnterPressed(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        if (ViewModel.SearchCommand.CanExecute(null))
        {
            ViewModel.SearchCommand.Execute(null);
        }
        args.Handled = true;
    }
}
