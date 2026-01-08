using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Vidora.Presentation.Gui.ViewModels;

namespace Vidora.Presentation.Gui.Views;

public sealed partial class HomePage : Page
{
    public HomeViewModel ViewModel { get; } = App.GetService<HomeViewModel>();

    public HomePage()
    {
        InitializeComponent();
        DataContext = ViewModel;
    }

    private void HeroSection_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (sender is FlipView flipView)
        {
            var newHeight = e.NewSize.Width / 4 * 3;
            flipView.Height = newHeight;
        }
    }
}
