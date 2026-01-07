using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Vidora.Core.Entities;
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
        IMapper mapper,
        IInfoBarService infoBarService,
        INavigationService navigationService)
    {
        _getMovieDetailUseCase = getMovieDetailUseCase;
        _addToWatchlistUseCase = addToWatchlistUseCase;
        _removeFromWatchlistUseCase = removeFromWatchlistUseCase;
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

                // ✅ Xóa 2 dòng hardcode này khi muốn dùng link thật từ API
                Movie.VideoUrl = "https://player.cloudinary.com/embed/?cloud_name=dv7hgu8yw&public_id=phim1_jgzhpj&profile=cld-default.mp4";    
                Movie.TrailerUrl = "https://res.cloudinary.com/df8meqyyc/video/upload/v1764442497/strangerthings5_pi1yuk.mp4";

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
}