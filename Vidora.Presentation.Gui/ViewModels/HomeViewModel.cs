using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Vidora.Core.UseCases;
using Vidora.Presentation.Gui.Contracts.Services;
using Vidora.Presentation.Gui.Contracts.ViewModels;
using Vidora.Presentation.Gui.Models;

namespace Vidora.Presentation.Gui.ViewModels;

public partial class HomeViewModel : ObservableRecipient, INavigationAware
{
    private readonly GetDashboardUseCase _getDashboardUseCase;
    private readonly IMapper _mapper;
    private readonly INavigationService _navigationService;

    private ObservableCollection<Movie> _heroItems = [];
    public ObservableCollection<Movie> HeroItems
    {
        get => _heroItems;
        set => SetProperty(ref _heroItems, value);
    }

    private int _selectedHeroIndex = 0;
    public int SelectedHeroIndex
    {
        get => _selectedHeroIndex;
        set => SetProperty(ref _selectedHeroIndex, value);
    }

    private ObservableCollection<Movie> _trendingItems = [];
    public ObservableCollection<Movie> TrendingItems
    {
        get => _trendingItems;
        set => SetProperty(ref _trendingItems, value);
    }

    private int _selectedTrendingIndex = 0;
    public int SelectedTrendingIndex
    {
        get => _selectedTrendingIndex;
        set => SetProperty(ref _selectedTrendingIndex, value);
    }

    private ObservableCollection<Movie> _recommendedItems = [];
    public ObservableCollection<Movie> RecommendedItems
    {
        get => _recommendedItems;
        set => SetProperty(ref _recommendedItems, value);
    }

    private int _selectedRecommendedIndex = 0;
    public int SelectedRecommendedIndex
    {
        get => _selectedRecommendedIndex;
        set => SetProperty(ref _selectedRecommendedIndex, value);
    }

    private ObservableCollection<Movie> _newReleaseItems = [];
    public ObservableCollection<Movie> NewReleaseItems
    {
        get => _newReleaseItems;
        set => SetProperty(ref _newReleaseItems, value);
    }

    private int _selectedNewReleaseIndex;
    public int SelectedNewReleaseIndex
    {
        get => _selectedNewReleaseIndex;
        set => SetProperty(ref _selectedNewReleaseIndex, value);
    }

    private ObservableCollection<Movie> _continueWatchingItems = [];
    public ObservableCollection<Movie> ContinueWatchingItems
    {
        get => _continueWatchingItems;
        set => SetProperty(ref _continueWatchingItems, value);
    }

    private int _selectedContinueWatchingIndex = 0;
    public int SelectedContinueWatchingIndex
    {
        get => _selectedContinueWatchingIndex;
        set => SetProperty(ref _selectedContinueWatchingIndex, value);
    }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    private bool _hasError;
    public bool HasError
    {
        get => _hasError;
        set => SetProperty(ref _hasError, value);
    }

    private string _errorMessage = string.Empty;
    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public HomeViewModel(
        GetDashboardUseCase getDashboardUseCase,
        IMapper mapper,
        INavigationService navigationService)
    {
        _getDashboardUseCase = getDashboardUseCase;
        _mapper = mapper;
        _navigationService = navigationService;
    }

    public async Task OnNavigatedToAsync(object parameter)
    {
        IsLoading = true;
        HasError = false;
        ErrorMessage = string.Empty;

        try
        {
            var result = await _getDashboardUseCase.ExecuteAsync();

            if (result.IsSuccess)
            {
                var dashboard = result.Value;

                // Get top 5 highest rated movies
                var highestRatedMovies = _mapper.Map<IReadOnlyList<Movie>>(dashboard.HighestRatedMovies);
                var topMovies = highestRatedMovies.Take(5).ToList();

                // Use top rated movies for Hero banner (max 4)
                HeroItems = new ObservableCollection<Movie>(topMovies.Take(4));

                // Use highest rated movies for Trending section
                TrendingItems = new ObservableCollection<Movie>(topMovies);

                // Use highest rated movies for Recommended section
                RecommendedItems = new ObservableCollection<Movie>(topMovies);

                // Use highest rated movies for New Releases section
                NewReleaseItems = new ObservableCollection<Movie>(topMovies);

                // Continue Watching - can be empty or use same data
                ContinueWatchingItems = new ObservableCollection<Movie>(topMovies);
            }
            else
            {
                HasError = true;
                ErrorMessage = result.Error;
                // Fallback to sample data if API fails
                LoadSampleData();
            }
        }
        catch (System.Exception ex)
        {
            HasError = true;
            ErrorMessage = ex.Message;
            // Fallback to sample data if exception occurs
            LoadSampleData();
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task OnNavigatedFromAsync()
    {
    }

    [RelayCommand]
    private async Task ViewMovieDetailAsync(Movie movie)
    {
        if (movie is not null)
        {
            await _navigationService.NavigateToAsync<MovieDetailViewModel>(movie.MovieId);
        }
    }

    [RelayCommand]
    private async Task PlayMovieAsync(Movie movie)
    {
        if (movie is not null)
        {
            // Navigate to video player with movie
            await _navigationService.NavigateToAsync<VideoPlayerViewModel>(movie.MovieId);
        }
    }

    private void LoadSampleData()
    {
        HeroItems = GetSampleFilm(4);
        TrendingItems = GetSampleFilm(10);
        RecommendedItems = GetSampleFilm(15);
        NewReleaseItems = GetSampleFilm(15);
        ContinueWatchingItems = GetSampleFilm(10);
    }

    private ObservableCollection<Movie> GetSampleFilm(int count)
    {
        if (count <= 0)
            return new ObservableCollection<Movie>();

        var sampleFilms = new List<Movie>
        {
            new Movie
            {
                Id = "1",
                Title = "Stranger Things",
                Description = "A thrilling Netflix series about supernatural events in Hawkins, Indiana.",
                PosterUrl = "https://dnm.nflximg.net/api/v6/2DuQlx0fM4wd1nzqm5BFBi6ILa8/AAAAQeHSBosv8l2X9RZuaT3ygZYs0XLLqa8vrpyBf1dTH8cjYR6sQsL26uyTNujyLkzvZKz3OyFvkd0u6PS-ZGcpyuRHnDLuYXucxhVMJxQXmZLlz88mnJ_jX5UymYsghaBfcEGD2RbIifQR7j4N5gGkvBDQ.jpg?r=473",
                BannerUrl = "https://4kwallpapers.com/images/wallpapers/stranger-things-2880x1800-23262.jpg",
                TrailerUrl = "https://res.cloudinary.com/df8meqyyc/video/upload/v1764442497/strangerthings5_pi1yuk.mp4",
                VideoUrl = "https://res.cloudinary.com/df8meqyyc/video/upload/v1764442497/strangerthings5_pi1yuk.mp4",
                Rating = 8.9,
                Genres = new List<string> { "Drama", "Mystery", "Sci-Fi" },
            },
            new Movie
            {
                Id = "2",
                Title = "Inception",
                Description = "A mind-bending thriller where dream invasion is possible.",
                PosterUrl = "https://bestthrillers.com/wp-content/uploads/2010/05/Inception-Movie-Poster.jpg",
                BannerUrl = "https://i0.wp.com/3.bp.blogspot.com/_o31CLSHm6KA/TA8B0jjTzQI/AAAAAAAAAE0/MlRL4UVSy9s/s1600/Inception%2Bbanner%2B4.jpg",
                TrailerUrl = "https://res.cloudinary.com/df8meqyyc/video/upload/v1764442497/strangerthings5_pi1yuk.mp4",
                VideoUrl = "https://res.cloudinary.com/df8meqyyc/video/upload/v1764442497/strangerthings5_pi1yuk.mp4",
                Rating = 8.8,
                Genres = new List<string> { "Action", "Sci-Fi", "Thriller" },
                DurationMinutes = 148
            },
            new Movie
            {
                Id = "3",
                Title = "Interstellar",
                Description = "A visually stunning sci-fi adventure exploring space and time.",
                PosterUrl = "https://mythicwall.com/cdn/shop/files/Interstellar_2BMovie_2B_2Bposter_2BPrint_2BWall_2BArt_2BPoster_2B1-W0pfS_1024x1024.jpg?v=1762442294",
                BannerUrl = "https://tintuc-divineshop.cdn.vccloud.vn/wp-content/uploads/2025/02/interstellar-3.jpg",
                TrailerUrl = "https://res.cloudinary.com/df8meqyyc/video/upload/v1764442497/strangerthings5_pi1yuk.mp4",
                VideoUrl = "https://res.cloudinary.com/df8meqyyc/video/upload/v1764442497/strangerthings5_pi1yuk.mp4",
                Rating = 8.6,
                Genres = new List<string> { "Adventure", "Drama", "Sci-Fi" },
                DurationMinutes = 169
            },
            new Movie
            {
                Id = "4",
                Title = "Avatar",
                Description = "A paraplegic Marine dispatched to the moon Pandora on a unique mission becomes torn between following his orders and protecting the world he feels is his home.",
                PosterUrl = "https://i.pinimg.com/736x/8b/2f/a6/8b2fa6fb94810cd0d335b479896f7fc8.jpg",
                BannerUrl = "https://www.thebanner.org/sites/default/files/styles/article_detail_header/public/2023-01/avatar-way-of-water.jpg?itok=3SeSRjXH",
                TrailerUrl = "https://res.cloudinary.com/df8meqyyc/video/upload/v1764442497/strangerthings5_pi1yuk.mp4",
                VideoUrl = "https://res.cloudinary.com/df8meqyyc/video/upload/v1764442497/strangerthings5_pi1yuk.mp4",
                Rating = 9.0,
                Genres = new List<string> { "Action", "Sci-Fi" },
                DurationMinutes = 148
            },
        };

        var result = new ObservableCollection<Movie>();
        for (int i = 0; i < count; i++)
        {
            var sample = sampleFilms[i % sampleFilms.Count];
            result.Add(new Movie
            {
                Id = sample.Id,
                Title = sample.Title,
                Description = sample.Description,
                PosterUrl = sample.PosterUrl,
                BannerUrl = sample.BannerUrl,
                TrailerUrl = sample.TrailerUrl,
                VideoUrl = sample.VideoUrl,
                Rating = sample.Rating,
                Genres = new List<string>(sample.Genres),
                DurationMinutes = sample.DurationMinutes
            });
        }

        return result;
    }
}
