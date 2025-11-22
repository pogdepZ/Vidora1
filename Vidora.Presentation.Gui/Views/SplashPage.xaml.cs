using Microsoft.UI.Xaml.Controls;
using Vidora.Presentation.Gui.ViewModels;

namespace Vidora.Presentation.Gui.Views;

public sealed partial class SplashPage : Page
{
    public SplashViewModel ViewModel { get; } = App.GetService<SplashViewModel>();
    public SplashPage()
    {
        InitializeComponent();
    }
}
