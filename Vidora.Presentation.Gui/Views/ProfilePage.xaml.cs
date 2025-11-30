using Microsoft.UI.Xaml.Controls;
using Vidora.Presentation.Gui.ViewModels;

namespace Vidora.Presentation.Gui.Views
{
    public sealed partial class ProfilePage : Page
    {
        public ProfileViewModel ViewModel { get; } = App.GetService<ProfileViewModel>();
        public ProfilePage()
        {
            InitializeComponent();
        }
    }
}
