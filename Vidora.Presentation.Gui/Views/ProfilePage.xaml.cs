using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Vidora.Presentation.Gui.ViewModels;

namespace Vidora.Presentation.Gui.Views;

public sealed partial class ProfilePage : Page
{
    // Khai báo ViewModel để x:Bind trong XAML sử dụng
    public ProfileViewModel ViewModel { get; } = App.GetService<ProfileViewModel>();
  
    public ProfilePage()
    {
        InitializeComponent();
    }

    // Chuyển thành 'async void' để await được hàm LoadData bên ViewModel
    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        // Gọi hàm Async bên ViewModel và chờ nó chạy xong
        await ViewModel.OnNavigatedToAsync(e.Parameter);
    }

    protected override async void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);

        // Gọi hàm cleanup Async bên ViewModel
        await ViewModel.OnNavigatedFromAsync();
    }
}