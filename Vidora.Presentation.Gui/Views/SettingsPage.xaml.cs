using Microsoft.UI.Xaml.Controls;
using Vidora.Presentation.Gui.ViewModels;

namespace Vidora.Presentation.Gui.Views;

public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel { get; } = App.GetService<SettingsViewModel>();
    public SettingsPage()
    {
        InitializeComponent();
    }
}
