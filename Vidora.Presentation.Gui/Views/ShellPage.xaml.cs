using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Vidora.Presentation.Gui.Contracts.Services;
using Vidora.Presentation.Gui.Helpers;
using Vidora.Presentation.Gui.ViewModels;

namespace Vidora.Presentation.Gui.Views;

public sealed partial class ShellPage : Page
{
    ShellViewModel ViewModel { get; } = App.GetService<ShellViewModel>();
    public ShellPage()
    {
        InitializeComponent();
        ViewModel.InfoBarService.Initialize(AppInfoBar);
        ViewModel.NavigationViewService.Initialize(AppNavigationView);
        
        App.MainWindow.ExtendsContentIntoTitleBar = true;
        App.MainWindow.SetTitleBar(AppTitleBar);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        TitleBarHelper.UpdateTitleBar(RequestedTheme);
    }
}
