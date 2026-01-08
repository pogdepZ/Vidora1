using Microsoft.UI.Xaml.Controls;
using Vidora.Presentation.Gui.ViewModels;

namespace Vidora.Presentation.Gui.Views;

public sealed partial class ManageSubscriptionsPage : Page
{
    public ManageSubscriptionsViewModel ViewModel { get; } = App.GetService<ManageSubscriptionsViewModel>();

    public ManageSubscriptionsPage()
    {
        InitializeComponent();
    }
}
