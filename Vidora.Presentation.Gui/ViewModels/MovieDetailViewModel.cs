using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Vidora.Core.UseCases;
using Vidora.Presentation.Gui.Contracts.Services;
using Vidora.Presentation.Gui.Contracts.ViewModels;
using Vidora.Presentation.Gui.Models;

using CoreMovie = Vidora.Core.Entities.Movie;
using GuiMovie = Vidora.Presentation.Gui.Models.Movie;

namespace Vidora.Presentation.Gui.ViewModels;

public partial class MovieDetailViewModel : ObservableRecipient, INavigationAware
{
    private readonly GetMovieDetailUseCase _getMovieDetailUseCase;
    private readonly AddToWatchlistUseCase _addToWatchlistUseCase;
    private readonly RemoveFromWatchlistUseCase _removeFromWatchlistUseCase;
    private readonly RateMovieUseCase _rateMovieUseCase;
    private readonly IMapper _mapper;
    private readonly IInfoBarService _infoBarService;
    private readonly INavigationService _navigationService;

    private int _movieId;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MovieTitle))]
    [NotifyPropertyChangedFor(nameof(MovieDescription))]
    [NotifyPropertyChangedFor(nameof(MoviePosterImage))]
    [NotifyPropertyChangedFor(nameof(MovieReleaseYear))]
    [NotifyPropertyChangedFor(nameof(MovieAvgRating))]
    private GuiMovie? _movie;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private string _genresDisplay = string.Empty;

    [ObservableProperty]
    private string _actorsDisplay = string.Empty;

    [ObservableProperty]
    private bool _hasTrailer;

    [ObservableProperty]
    private bool _hasMovie;

    [ObservableProperty]
    private bool _isInWatchlist;

    [ObservableProperty]
    private bool _isTogglingWatchlist;

    // Rating properties
    [ObservableProperty]
    private bool _showRatingPanel;

    [ObservableProperty]
    private int _selectedRating;

    [ObservableProperty]
    private int _hoveredRating;

    [ObservableProperty]
    private bool _isSubmittingRating;

    [ObservableProperty]
    private int _userRating;

    // Collection của các sao (1-10)
    public ObservableCollection<int> Stars { get; } = new(Enumerable.Range(1, 10));

    // Wrapper properties for null-safe XAML bindings
    public string MovieTitle => Movie?.Title ?? "Đang tải...";
    public string MovieDescription => Movie?.Description ?? "Chưa có mô tả";
    public ImageSource MoviePosterImage
    {
        get
        {
            if (string.IsNullOrEmpty(Movie?.PosterUrl))
            {
                return new BitmapImage(new Uri("ms-appx:///Assets/no-image.png"));
            }

            return new BitmapImage(new Uri(Movie.PosterUrl));
        }
    }
    public string MovieReleaseYear => Movie?.ReleaseYear?.ToString() ?? "----";
    public double MovieAvgRating => Movie?.AvgRating ?? 0;

    public MovieDetailViewModel(
        GetMovieDetailUseCase getMovieDetailUseCase,
        AddToWatchlistUseCase addToWatchlistUseCase,
        RemoveFromWatchlistUseCase removeFromWatchlistUseCase,
        RateMovieUseCase rateMovieUseCase,
        IMapper mapper,
        IInfoBarService infoBarService,
        INavigationService navigationService)
    {
        _getMovieDetailUseCase = getMovieDetailUseCase;
        _addToWatchlistUseCase = addToWatchlistUseCase;
        _removeFromWatchlistUseCase = removeFromWatchlistUseCase;
        _rateMovieUseCase = rateMovieUseCase;
        _mapper = mapper;
        _infoBarService = infoBarService;
        _navigationService = navigationService;
    }

    public async Task OnNavigatedToAsync(object parameter)
    {
        if (parameter is int movieId)
        {
            _movieId = movieId;
            await LoadMovieDetailAsync();
        }
        else
        {
            HasError = true;
            ErrorMessage = "Invalid movie ID";
        }
    }

    public Task OnNavigatedFromAsync()
    {
        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task LoadMovieDetailAsync()
    {
        if (IsLoading) return;

        try
        {
            IsLoading = true;
            HasError = false;
            ErrorMessage = string.Empty;

            var result = await _getMovieDetailUseCase.ExecuteAsync(_movieId);

            if (result.IsSuccess)
            {
                Movie = _mapper.Map<CoreMovie, GuiMovie>(result.Value.Movie);


                GenresDisplay = Movie.Genres.Count > 0
                    ? string.Join(" • ", Movie.Genres)
                    : "Chưa cập nhật";

                ActorsDisplay = (Movie.Actors != null && Movie.Actors.Any())
                    ? string.Join(", ", Movie.Actors.Select(a => a.Name))
                    : "Không có thông tin diễn viên";

                HasTrailer = !string.IsNullOrEmpty(Movie.TrailerUrl);
                HasMovie = !string.IsNullOrEmpty(Movie.VideoUrl);

                // TODO: Load watchlist status from API
                // IsInWatchlist = result.Value.IsInWatchlist;
            }
            else
            {
                HasError = true;
                ErrorMessage = result.Error;
            }
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Lỗi tải thông tin phim: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await _navigationService.GoBackAsync();
    }

    [RelayCommand]
    private async Task PlayMovieAsync()
    {
        if (Movie?.VideoUrl is not null)
        {
            await _navigationService.NavigateToAsync<VideoPlayerViewModel>(new { Url = Movie.VideoUrl, Title = Movie.Title });
        }
        else
        {
            _infoBarService.ShowWarning("Phim chưa có sẵn");
        }
    }

    [RelayCommand]
    private async Task PlayTrailerAsync()
    {
        if (Movie?.TrailerUrl is not null)
        {
            await _navigationService.NavigateToAsync<VideoPlayerViewModel>(new { Url = Movie.TrailerUrl, Title = $"Trailer: {Movie.Title}" });
        }
        else
        {
            _infoBarService.ShowWarning("Trailer chưa có sẵn");
        }
    }

    [RelayCommand]
    private async Task ToggleWatchlistAsync()
    {
        if (IsTogglingWatchlist || _movieId <= 0) return;

        try
        {
            IsTogglingWatchlist = true;

            // Thay toggle bằng logic Add/Remove
            if (IsInWatchlist)
            {
                // Xóa khỏi watchlist
                var result = await _removeFromWatchlistUseCase.ExecuteAsync(_movieId);

                if (result.IsSuccess)
                {
                    IsInWatchlist = false;
                    _infoBarService.ShowInfo("Đã xóa khỏi danh sách yêu thích");
                }
                else
                {
                    _infoBarService.ShowError(result.Error);
                }
            }
            else
            {
                // Thêm vào watchlist
                var result = await _addToWatchlistUseCase.ExecuteAsync(_movieId);

                if (result.IsSuccess)
                {
                    IsInWatchlist = true;
                    _infoBarService.ShowSuccess("Đã thêm vào danh sách yêu thích");
                }
                else
                {
                    _infoBarService.ShowError(result.Error);
                }
            }
        }
        catch (Exception ex)
        {
            _infoBarService.ShowError($"Lỗi: {ex.Message}");
        }
        finally
        {
            IsTogglingWatchlist = false;
        }
    }

    [RelayCommand]
    private void Share()
    {
        _infoBarService.ShowInfo("Đã sao chép link chia sẻ");
        // TODO: Implement share functionality
    }

    // ===== RATING COMMANDS =====

    [RelayCommand]
    private void OpenRatingPanel()
    {
        ShowRatingPanel = true;
        SelectedRating = UserRating > 0 ? UserRating : 5; // Mặc định 5 sao nếu chưa đánh giá
        HoveredRating = 0;
    }

    [RelayCommand]
    private void CloseRatingPanel()
    {
        ShowRatingPanel = false;
        HoveredRating = 0;
    }

    [RelayCommand]
    private void HoverStar(object? parameter)
    {
        if (parameter is int star)
        {
            HoveredRating = star;
        }
        else if (parameter is string starStr && int.TryParse(starStr, out int parsedStar))
        {
            HoveredRating = parsedStar;
        }
    }

    [RelayCommand]
    private void LeaveStar()
    {
        HoveredRating = 0;
    }

    [RelayCommand]
    private void SelectStar(object? parameter)
    {
        if (parameter is int star)
        {
            SelectedRating = star;
        }
        else if (parameter is string starStr && int.TryParse(starStr, out int parsedStar))
        {
            SelectedRating = parsedStar;
        }
    }

    [RelayCommand]
    private async Task SubmitRatingAsync()
    {
        if (IsSubmittingRating || SelectedRating <= 0 || _movieId <= 0) return;

        try
        {
            IsSubmittingRating = true;

            var result = await _rateMovieUseCase.ExecuteAsync(_movieId, SelectedRating);

            if (result.IsSuccess)
            {
                UserRating = SelectedRating;
                ShowRatingPanel = false;
                _infoBarService.ShowSuccess($"Đã đánh giá {SelectedRating}/10 ⭐");

                // Reload để cập nhật điểm trung bình mới
                await LoadMovieDetailAsync();
            }
            else
            {
                _infoBarService.ShowError(result.Error);
            }
        }
        catch (Exception ex)
        {
            _infoBarService.ShowError($"Lỗi: {ex.Message}");
        }
        finally
        {
            IsSubmittingRating = false;
        }
    }
}