using Microsoft.UI.Xaml.Controls;
using Vidora.Presentation.Gui.ViewModels;

namespace Vidora.Presentation.Gui.Views;

public sealed partial class VideoPlayerPage : Page
{
    public VideoPlayerViewModel ViewModel { get; } = App.GetService<VideoPlayerViewModel>();
    public VideoPlayerPage()
    {
        InitializeComponent();
    }
}
