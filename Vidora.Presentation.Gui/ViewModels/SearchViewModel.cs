using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Vidora.Core.Contracts.Commands;
using Vidora.Core.Contracts.Results;
using Vidora.Core.UseCases;
using Vidora.Presentation.Gui.Contracts.Services;
using Vidora.Presentation.Gui.Contracts.ViewModels;
using Vidora.Presentation.Gui.Models;

namespace Vidora.Presentation.Gui.ViewModels;

public partial class SearchViewModel : ObservableRecipient, INavigationAware
{
    private readonly SearchMovieUseCase _searchMovieUseCase;
    private readonly GetGenresUseCase _getGenresUseCase;
    private readonly IMapper _mapper;
    private readonly IInfoBarService _infoBarService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private Genre? _selectedGenre;

    [ObservableProperty]
    private int? _selectedYear;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isLoadingGenres;

    [ObservableProperty]
    private bool _hasResults;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages = 1;

    [ObservableProperty]
    private int _totalResults;

    [ObservableProperty]
    private bool _canGoPrevious;

    [ObservableProperty]
    private bool _canGoNext;

    [ObservableProperty]
    private Movie? _hoveredMovie;

    public ObservableCollection<Movie> SearchResults { get; } = [];

    public ObservableCollection<Genre> Genres { get; } = [];

    public ObservableCollection<int> Years { get; } = [];

    public SearchViewModel(
        SearchMovieUseCase searchMovieUseCase,
        GetGenresUseCase getGenresUseCase,
        IMapper mapper,
        IInfoBarService infoBarService,
        INavigationService navigationService)
    {
        _searchMovieUseCase = searchMovieUseCase;
        _getGenresUseCase = getGenresUseCase;
        _mapper = mapper;
        _infoBarService = infoBarService;
        _navigationService = navigationService;

        // Populate years from current year to 1990
        var currentYear = DateTime.Now.Year;
        for (var year = currentYear; year >= 1990; year--)
        {
            Years.Add(year);
        }
    }

    public async Task OnNavigatedToAsync(object parameter)
    {
        await LoadGenresAsync();
        await SearchAsync();
    }

    public Task OnNavigatedFromAsync()
    {
        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task LoadGenresAsync()
    {
        if (IsLoadingGenres || Genres.Count > 0) return;

        try
        {
            IsLoadingGenres = true;

            var result = await _getGenresUseCase.ExecuteAsync();

            if (result.IsSuccess)
            {
                Genres.Clear();
                foreach (var genre in result.Value.Genres)
                {
                    var mappedGenre = _mapper.Map<Genre>(genre);
                    Genres.Add(mappedGenre);
                }
            }
        }
        catch (Exception)
        {
            // Silent fail - genres will just not be available
        }
        finally
        {
            IsLoadingGenres = false;
        }
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        CurrentPage = 1;
        await LoadMoviesAsync();
    }

    [RelayCommand]
    private async Task LoadMoviesAsync()
    {
        if (IsLoading) return;

        try
        {
            IsLoading = true;

            var command = new SearchMovieCommand
            {
                Title = string.IsNullOrWhiteSpace(SearchQuery) ? null : SearchQuery.Trim(),
                GenreId = SelectedGenre?.GenreId,
                ReleaseYear = SelectedYear,
                Page = CurrentPage,
                Limit = 20
            };

            var result = await _searchMovieUseCase.ExecuteAsync(command);

            if (result.IsSuccess)
            {
                UpdateSearchResults(result.Value);
            }
            else
            {
                _infoBarService.ShowError(result.Error);
                ClearResults();
            }
        }
        catch (Exception ex)
        {
            _infoBarService.ShowError($"Lỗi tìm kiếm: {ex.Message}");
            ClearResults();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void UpdateSearchResults(SearchMovieResult result)
    {
        SearchResults.Clear();

        foreach (var movie in result.Movies)
        {
            var mappedMovie = _mapper.Map<Movie>(movie);
            SearchResults.Add(mappedMovie);
        }

        TotalResults = result.Pagination.Total;
        TotalPages = result.Pagination.TotalPages;
        CurrentPage = result.Pagination.Page;
        HasResults = SearchResults.Count > 0;
        CanGoPrevious = result.Pagination.HasPrev;
        CanGoNext = result.Pagination.HasNext;
    }

    private void ClearResults()
    {
        SearchResults.Clear();
        HasResults = false;
        TotalResults = 0;
        TotalPages = 1;
        CanGoPrevious = false;
        CanGoNext = false;
    }

    [RelayCommand]
    private async Task PreviousPageAsync()
    {
        if (CanGoPrevious)
        {
            CurrentPage--;
            await LoadMoviesAsync();
        }
    }

    [RelayCommand]
    private async Task NextPageAsync()
    {
        if (CanGoNext)
        {
            CurrentPage++;
            await LoadMoviesAsync();
        }
    }

    [RelayCommand]
    private async Task ClearFiltersAsync()
    {
        SearchQuery = string.Empty;
        SelectedGenre = null;
        SelectedYear = null;
        await SearchAsync();
    }

    [RelayCommand]
    private async Task ViewMovieDetailAsync(Movie movie)
    {
        if (movie is not null)
        {
            await _navigationService.NavigateToAsync<MovieDetailViewModel>(movie.MovieId);
        }
    }
}