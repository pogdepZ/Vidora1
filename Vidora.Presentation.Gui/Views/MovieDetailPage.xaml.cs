using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Vidora.Presentation.Gui.ViewModels;

namespace Vidora.Presentation.Gui.Views;

public sealed partial class MovieDetailPage : Page
{
    public MovieDetailViewModel ViewModel { get; } = App.GetService<MovieDetailViewModel>();
    
    private FontIcon[]? _starIcons;
    private readonly SolidColorBrush _goldBrush = new(Colors.Gold);
    private SolidColorBrush? _grayBrush;

    public MovieDetailPage()
    {
        InitializeComponent();
        
        // Subscribe to property changes
        ViewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    private void EnsureStarIconsInitialized()
    {
        if (_starIcons == null)
        {
            if (Star1 != null)
            {
                _starIcons = [Star1, Star2, Star3, Star4, Star5, Star6, Star7, Star8, Star9, Star10];
                _grayBrush = Application.Current.Resources.TryGetValue("TextFillColorTertiaryBrush", out var brush) 
                    ? brush as SolidColorBrush 
                    : new SolidColorBrush(Colors.Gray);
            }
        }
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModel.SelectedRating) || 
            e.PropertyName == nameof(ViewModel.HoveredRating) ||
            e.PropertyName == nameof(ViewModel.ShowRatingPanel))
        {
            if (ViewModel.ShowRatingPanel)
            {
                // Ð?i m?t chút ð? UI render xong
                DispatcherQueue.TryEnqueue(() =>
                {
                    EnsureStarIconsInitialized();
                    UpdateStarColors();
                });
            }
        }
    }

    private void UpdateStarColors()
    {
        if (_starIcons == null || _grayBrush == null) return;
        
        var activeRating = ViewModel.HoveredRating > 0 ? ViewModel.HoveredRating : ViewModel.SelectedRating;
        
        for (int i = 0; i < _starIcons.Length; i++)
        {
            if (_starIcons[i] != null)
            {
                _starIcons[i].Foreground = i < activeRating ? _goldBrush : _grayBrush;
            }
        }
    }

    private void Star_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string tagStr && int.TryParse(tagStr, out int star))
        {
            ViewModel.HoverStarCommand.Execute(star);
        }
    }

    private void Star_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        ViewModel.LeaveStarCommand.Execute(null);
    }
}
