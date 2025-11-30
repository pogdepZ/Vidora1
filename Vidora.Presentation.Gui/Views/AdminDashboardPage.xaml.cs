using Microsoft.UI.Xaml.Controls;
using Vidora.Presentation.Gui.ViewModels;

namespace Vidora.Presentation.Gui.Views;

public sealed partial class AdminDashboardPage : Page
{
    public AdminDashboardViewModel ViewModel { get; } = App.GetService<AdminDashboardViewModel>();
    public AdminDashboardPage()
    {
        InitializeComponent();
    }
}
