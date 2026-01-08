using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Vidora.Presentation.Gui.ViewModels;

namespace Vidora.Presentation.Gui.Views;

public sealed partial class ProfilePage : Page
{
    public ProfileViewModel ViewModel { get; } = App.GetService<ProfileViewModel>();
  
    public ProfilePage()
    {
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        await ViewModel.OnNavigatedToAsync(e.Parameter);
    }

    protected override async void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);

        await ViewModel.OnNavigatedFromAsync();
    }
}
