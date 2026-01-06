using Microsoft.UI.Xaml.Controls;
using Vidora.Presentation.Gui.ViewModels;

namespace Vidora.Presentation.Gui.Views;

public sealed partial class CheckoutPage : Page
{
    public CheckoutViewModel ViewModel { get; } = App.GetService<CheckoutViewModel>();
    
    public CheckoutPage()
    {
        InitializeComponent();
        DataContext = ViewModel;
    }
}
