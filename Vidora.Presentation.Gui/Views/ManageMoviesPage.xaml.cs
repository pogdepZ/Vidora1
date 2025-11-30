using Microsoft.UI.Xaml.Controls;
using Vidora.Presentation.Gui.ViewModels;

namespace Vidora.Presentation.Gui.Views;

public sealed partial class ManageMoviesPage : Page
{
    public ManageMoviesViewModel ViewModel { get; } = App.GetService<ManageMoviesViewModel>();
    public ManageMoviesPage()
    {
        InitializeComponent();
    }
}
