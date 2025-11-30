using Microsoft.UI.Xaml.Controls;
using Vidora.Presentation.Gui.ViewModels;

namespace Vidora.Presentation.Gui.Views;

public sealed partial class MovieDetailPage : Page
{
    public MovieDetailViewModel ViewModel { get; } = App.GetService<MovieDetailViewModel>();
    public MovieDetailPage()
    {
        InitializeComponent();
    }
}
