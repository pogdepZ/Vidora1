using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
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

public partial class WatchlistViewModel : ObservableRecipient, INavigationAware
{
    private readonly GetWatchlistUseCase _getWatchlistUseCase;
    private readonly RemoveFromWatchlistUseCase _removeFromWatchlistUseCase;
    private readonly IMapper _mapper;
    private readonly IInfoBarService _infoBarService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _hasMovies;

    [ObservableProperty]
    private int _totalMovies;

    // Danh sách phim gốc (để lưu trữ tất cả phim)
    private readonly List<GuiMovie> _allMovies = [];

    // Danh sách phim theo thể loại
    public ObservableCollection<GenreMovieGroup> GenreGroups { get; } = [];

    public WatchlistViewModel(
        GetWatchlistUseCase getWatchlistUseCase,
        RemoveFromWatchlistUseCase removeFromWatchlistUseCase,
        IMapper mapper,
        IInfoBarService infoBarService,
        INavigationService navigationService)
    {
        _getWatchlistUseCase = getWatchlistUseCase;
        _removeFromWatchlistUseCase = removeFromWatchlistUseCase;
        _mapper = mapper;
        _infoBarService = infoBarService;
        _navigationService = navigationService;
    }

    public async Task OnNavigatedToAsync(object parameter)
    {
        await LoadWatchlistAsync();
    }

    public Task OnNavigatedFromAsync()
    {
        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task LoadWatchlistAsync()
    {
        if (IsLoading) return;

        try
        {
            IsLoading = true;
            _allMovies.Clear();
            GenreGroups.Clear();

            // Load tất cả phim (có thể load nhiều trang nếu cần)
            var page = 1;
            var hasMore = true;

            while (hasMore)
            {
                var result = await _getWatchlistUseCase.ExecuteAsync(page, 50);

                if (result.IsSuccess)
                {
                    foreach (var movie in result.Value.Movies)
                    {
                        var mappedMovie = _mapper.Map<CoreMovie, GuiMovie>(movie);
                        _allMovies.Add(mappedMovie);
                    }

                    TotalMovies = result.Value.Pagination.Total;
                    hasMore = result.Value.Pagination.HasNext;
                    page++;
                }
                else
                {
                    await _infoBarService.ShowErrorAsync(result.Error);
                    hasMore = false;
                }
            }

            // Nhóm phim theo thể loại
            GroupMoviesByGenre();

            HasMovies = _allMovies.Count > 0;
        }
        catch (Exception ex)
        {
            await _infoBarService.ShowErrorAsync($"Lỗi tải danh sách: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void GroupMoviesByGenre()
    {
        GenreGroups.Clear();

        // Lấy tất cả thể loại unique từ các phim
        var genreMoviesDict = new Dictionary<string, List<GuiMovie>>();

        foreach (var movie in _allMovies)
        {
            if (movie.Genres == null || movie.Genres.Count == 0)
            {
                // Phim không có thể loại -> đưa vào "Khác"
                if (!genreMoviesDict.ContainsKey("Khác"))
                {
                    genreMoviesDict["Khác"] = [];
                }
                genreMoviesDict["Khác"].Add(movie);
            }
            else
            {
                foreach (var genre in movie.Genres)
                {
                    if (!genreMoviesDict.ContainsKey(genre))
                    {
                        genreMoviesDict[genre] = [];
                    }
                    // Chỉ thêm nếu chưa có trong danh sách (tránh trùng lặp)
                    if (!genreMoviesDict[genre].Any(m => m.MovieId == movie.MovieId))
                    {
                        genreMoviesDict[genre].Add(movie);
                    }
                }
            }
        }

        // Sắp xếp theo số lượng phim giảm dần
        var sortedGenres = genreMoviesDict
            .OrderByDescending(g => g.Value.Count)
            .ToList();

        foreach (var kvp in sortedGenres)
        {
            var group = new GenreMovieGroup
            {
                GenreName = kvp.Key,
                Movies = new ObservableCollection<GuiMovie>(kvp.Value)
            };
            GenreGroups.Add(group);
        }
    }

    [RelayCommand]
    private async Task RemoveFromWatchlistAsync(GuiMovie movie)
    {
        if (movie == null) return;

        try
        {
            var result = await _removeFromWatchlistUseCase.ExecuteAsync(movie.MovieId);

            if (result.IsSuccess)
            {
                // Xóa phim khỏi tất cả các group
                foreach (var group in GenreGroups.ToList())
                {
                    var movieToRemove = group.Movies.FirstOrDefault(m => m.MovieId == movie.MovieId);
                    if (movieToRemove != null)
                    {
                        group.Movies.Remove(movieToRemove);
                    }

                    // Xóa group nếu không còn phim
                    if (group.Movies.Count == 0)
                    {
                        GenreGroups.Remove(group);
                    }
                }

                // Xóa khỏi danh sách gốc
                _allMovies.RemoveAll(m => m.MovieId == movie.MovieId);
                TotalMovies = _allMovies.Count;
                HasMovies = _allMovies.Count > 0;

                await _infoBarService.ShowSuccessAsync($"Đã xóa \"{movie.Title}\" khỏi danh sách yêu thích");
            }
            else
            {
                await _infoBarService.ShowErrorAsync(result.Error);
            }
        }
        catch (Exception ex)
        {
            await _infoBarService.ShowErrorAsync($"Lỗi: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task ViewMovieDetailAsync(GuiMovie movie)
    {
        if (movie == null) return;
        await _navigationService.NavigateToAsync<MovieDetailViewModel>(movie.MovieId);
    }
}
