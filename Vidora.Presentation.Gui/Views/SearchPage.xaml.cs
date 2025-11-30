using Microsoft.UI.Xaml.Controls;
using Vidora.Presentation.Gui.ViewModels;

namespace Vidora.Presentation.Gui.Views;

public sealed partial class SearchPage : Page
{
    public SearchViewModel ViewModel { get; } = App.GetService<SearchViewModel>();
    public SearchPage()
    {
        InitializeComponent();
    }
}
