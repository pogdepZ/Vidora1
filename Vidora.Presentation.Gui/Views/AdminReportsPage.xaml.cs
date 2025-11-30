using Microsoft.UI.Xaml.Controls;
using Vidora.Presentation.Gui.ViewModels;

namespace Vidora.Presentation.Gui.Views;

public sealed partial class AdminReportsPage : Page
{
    public AdminReportsViewModel ViewModel { get; } = App.GetService<AdminReportsViewModel>();
    public AdminReportsPage()
    {
        InitializeComponent();
    }
}