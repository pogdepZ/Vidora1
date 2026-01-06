using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
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
    private bool _isLoadingMore;

    [ObservableProperty]
    private bool _hasMovies;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private bool _hasMorePages;

    [ObservableProperty]
    private int _totalMovies;

    public ObservableCollection<GuiMovie> Movies { get; } = [];

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
            CurrentPage = 1;
            Movies.Clear();

            var result = await _getWatchlistUseCase.ExecuteAsync(CurrentPage, 10);

            if (result.IsSuccess)
            {
                foreach (var movie in result.Value.Movies)
                {
                    var mappedMovie = _mapper.Map<CoreMovie, GuiMovie>(movie);
                    Movies.Add(mappedMovie);
                }

                TotalMovies = result.Value.Pagination.Total;
                HasMorePages = result.Value.Pagination.HasNext;
                HasMovies = Movies.Count > 0;
            }
            else
            {
                _infoBarService.ShowError(result.Error);
            }
        }
        catch (Exception ex)
        {
            _infoBarService.ShowError($"Lỗi tải danh sách: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task LoadMoreAsync()
    {
        if (IsLoadingMore || !HasMorePages) return;

        try
        {
            IsLoadingMore = true;
            CurrentPage++;

            var result = await _getWatchlistUseCase.ExecuteAsync(CurrentPage, 10);

            if (result.IsSuccess)
            {
                foreach (var movie in result.Value.Movies)
                {
                    var mappedMovie = _mapper.Map<CoreMovie, GuiMovie>(movie);
                    Movies.Add(mappedMovie);
                }

                HasMorePages = result.Value.Pagination.HasNext;
            }
            else
            {
                CurrentPage--;
                _infoBarService.ShowError(result.Error);
            }
        }
        catch (Exception ex)
        {
            CurrentPage--;
            _infoBarService.ShowError($"Lỗi tải thêm: {ex.Message}");
        }
        finally
        {
            IsLoadingMore = false;
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
                Movies.Remove(movie);
                TotalMovies--;
                HasMovies = Movies.Count > 0;
                _infoBarService.ShowSuccess($"Đã xóa \"{movie.Title}\" khỏi danh sách yêu thích");
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
    }

    [RelayCommand]
    private async Task ViewMovieDetailAsync(GuiMovie movie)
    {
        if (movie == null) return;
        await _navigationService.NavigateToAsync<MovieDetailViewModel>(movie.MovieId);
    }
}
