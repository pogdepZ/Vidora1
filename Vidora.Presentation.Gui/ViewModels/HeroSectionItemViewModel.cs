using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using Vidora.Presentation.Gui.Contracts.Services;
using Vidora.Presentation.Gui.Models;

namespace Vidora.Presentation.Gui.ViewModels;

public partial class HeroSectionItemViewModel : ObservableRecipient
{
    private Movie? _movie;
    public Movie? Movie
    {
        get => _movie;
        set => SetProperty(ref _movie, value);
    }
    public bool CanPlay => !string.IsNullOrEmpty(Movie?.VideoUrl);
    public bool CanWatchTrailer => string.IsNullOrEmpty(Movie?.VideoUrl)
                                   && !string.IsNullOrEmpty(Movie?.TrailerUrl);


    private readonly INavigationService _navigationService;
    public HeroSectionItemViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }


    [RelayCommand]
    public async Task PlayVideoAsync(string videoUrl)
    {
        if (!string.IsNullOrWhiteSpace(videoUrl))
        {
            await _navigationService.NavigateToAsync<VideoPlayerViewModel>(videoUrl);
        }
    }
}
