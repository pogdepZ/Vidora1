using Microsoft.UI.Xaml.Controls;
using Vidora.Presentation.Gui.ViewModels;

namespace Vidora.Presentation.Gui.Views;

public sealed partial class WatchlistPage : Page
{
    public WatchlistViewModel ViewModel { get; } = App.GetService<WatchlistViewModel>();
    public WatchlistPage()
    {
        InitializeComponent();
    }
}
