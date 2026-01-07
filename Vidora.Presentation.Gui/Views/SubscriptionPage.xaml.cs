using Microsoft.UI.Xaml.Controls;
using Vidora.Presentation.Gui.ViewModels;

namespace Vidora.Presentation.Gui.Views;

public sealed partial class SubscriptionPage : Page
{
    public SubscriptionViewModel ViewModel { get; } = App.GetService<SubscriptionViewModel>();
    public SubscriptionPage()
    {
        InitializeComponent();
        DataContext = ViewModel;
    }
}
