using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;

namespace Vidora.Presentation.Gui;

public sealed partial class MainWindow : Window
{
    private const int _minHeight = 600;
    private const int _minWidth = 800;
    private const int _defaultWidth = 1280;
    private const int _defaultHeight = 800;

    public MainWindow()
    {
        InitializeComponent();

        AppWindow.Resize(new Windows.Graphics.SizeInt32(_defaultWidth, _defaultHeight));

        if (AppWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.PreferredMinimumWidth = _minWidth;
            presenter.PreferredMinimumHeight = _minHeight;
            AppWindow.SetPresenter(presenter);
        }
    }
}
