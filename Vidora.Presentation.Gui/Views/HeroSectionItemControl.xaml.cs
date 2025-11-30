using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Vidora.Presentation.Gui.Models;
using Vidora.Presentation.Gui.ViewModels;

namespace Vidora.Presentation.Gui.Views;

public sealed partial class HeroSectionItemControl : UserControl
{
    public Movie Movie
    {
        get => (Movie)GetValue(MovieProperty);
        set => SetValue(MovieProperty, value);
    }

    public DependencyProperty MovieProperty { get; } = DependencyProperty.Register(
        nameof(Movie),
        typeof(Movie),
        typeof(HeroSectionItemControl),
        new PropertyMetadata(null, OnMovieChanged));

    private static void OnMovieChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HeroSectionItemControl control && e.NewValue is Movie newMovie)
        {
            control.ViewModel.Movie = newMovie;
        }
    }

    public HeroSectionItemViewModel ViewModel { get; } = App.GetService<HeroSectionItemViewModel>();
    public HeroSectionItemControl()
    {
        InitializeComponent();
    }
}
