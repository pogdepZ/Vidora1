using Microsoft.UI.Xaml.Controls;
using Vidora.Presentation.Gui.ViewModels;

namespace Vidora.Presentation.Gui.Views;

public sealed partial class ManageUsersPage : Page
{
    public ManageUsersViewModel ViewModel { get; } = App.GetService<ManageUsersViewModel>();

    public ManageUsersPage()
    {
        InitializeComponent();
    }
}
