using ClosedXML.Excel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Vidora.Core.Contracts.Services;
using Vidora.Presentation.Gui.Contracts.ViewModels;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace Vidora.Presentation.Gui.ViewModels;

public partial class ManageMoviesViewModel : ObservableRecipient, INavigationAware
{
    private static readonly JsonSerializerOptions JsonLowerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static readonly JsonSerializerOptions JsonInsensitiveOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private const string BaseUrl = "http://localhost:3000";

    private readonly ISessionStateService _sessionState;
    private readonly HttpClient _http;

    private readonly int _limit = 10;

    public ObservableCollection<MovieRow> Movies { get; } = new();
    public ObservableCollection<GenreDto> Genres { get; } = new();

    [ObservableProperty]
    private GenreDto? _selectedGenre;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _releaseYearText = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanPrev))]
    [NotifyPropertyChangedFor(nameof(CanNext))]
    [NotifyPropertyChangedFor(nameof(CurrentPageText))]
    private int _page = 1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanNext))]
    [NotifyPropertyChangedFor(nameof(TotalPagesText))]
    private int _totalPages = 1;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isInfoOpen;

    [ObservableProperty]
    private string _infoMessage = string.Empty;

    [ObservableProperty]
    private string _infoTitle = "Info";

    [ObservableProperty]
    private InfoBarSeverity _infoSeverity = InfoBarSeverity.Success;

    [ObservableProperty]
    private bool _isImportVisible;

    [ObservableProperty]
    private string _importStatus = string.Empty;

    [ObservableProperty]
    private int _importSuccess;

    [ObservableProperty]
    private int _importFailed;

    [ObservableProperty]
    private int _importCurrent;

    [ObservableProperty]
    private int _importTotal;

    [ObservableProperty]
    private double _importProgress;

    public bool CanPrev => Page > 1;
    public bool CanNext => Page < TotalPages;
    public string CurrentPageText => Page.ToString();
    public string TotalPagesText => TotalPages.ToString();

    public ManageMoviesViewModel(ISessionStateService sessionState)
    {
        _sessionState = sessionState;
        _http = new HttpClient();
    }

    public async Task OnNavigatedToAsync(object parameter)
    {
        await LoadGenresAsync();
        await LoadMoviesAsync(resetPage: true);
    }

    public async Task OnNavigatedFromAsync()
    {
    }

    public void SearchQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        if (SearchCommand.CanExecute(null))
        {
            SearchCommand.Execute(null);
        }
    }

    [RelayCommand]
    private async Task SearchAsync()
        => await LoadMoviesAsync(resetPage: true);

    [RelayCommand]
    private async Task ClearAsync()
    {
        SearchText = string.Empty;
        ReleaseYearText = string.Empty;
        SelectedGenre = null;
        await LoadMoviesAsync(resetPage: true);
    }

    [RelayCommand]
    private async Task PrevAsync()
    {
        if (Page <= 1)
            return;

        Page--;
        await LoadMoviesAsync(resetPage: false);
    }

    [RelayCommand]
    private async Task NextAsync()
    {
        if (Page >= TotalPages)
            return;

        Page++;
        await LoadMoviesAsync(resetPage: false);
    }

    [RelayCommand]
    private async Task DeleteMovieAsync(int movieId)
    {
        try
        {
            SetAuthHeader();

            var req = new HttpRequestMessage(HttpMethod.Patch, $"{BaseUrl}/api/movies/{movieId}/toggle-delete");
            var res = await _http.SendAsync(req);

            if (!res.IsSuccessStatusCode)
            {
                var body = await res.Content.ReadAsStringAsync();
                ShowInfo($"Toggle delete fail: {(int)res.StatusCode} {body}", isError: true);
                return;
            }

            ShowInfo("Đã toggle delete movie.", isError: false);
            await LoadMoviesAsync(resetPage: false);
        }
        catch (Exception ex)
        {
            ShowInfo($"Toggle delete error: {ex.Message}", isError: true);
        }
    }

    [RelayCommand]
    private async Task ViewEditMovieAsync(int movieId)
    {
        try
        {
            SetAuthHeader();

            var json = await _http.GetStringAsync($"{BaseUrl}/api/movies/{movieId}");
            var detailRes = JsonSerializer.Deserialize<ApiSingleResponse<MovieDetailDto>>(json, JsonInsensitiveOptions);

            var movieDetail = detailRes?.Data;
            if (detailRes?.Success != true || movieDetail == null)
            {
                ShowInfo("Không thể tải thông tin chi tiết phim.", isError: true);
                return;
            }

            var availableGenres = Genres.ToList();
            var availableMembers = await LoadMembersAsync();

            var selectedGenres = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (movieDetail.Genres != null)
            {
                foreach (var g in movieDetail.Genres)
                {
                    selectedGenres.Add(g);
                }
            }

            var selectedCast = new ObservableCollection<(MemberResult Member, string Role)>();

            var stackPanel = new StackPanel { Width = 500, Padding = new Thickness(0, 0, 15, 0) };

            var titleBox = new TextBox { Header = "Tiêu đề phim", Text = movieDetail.Title ?? string.Empty, Margin = new Thickness(0, 0, 0, 10) };
            var descBox = new TextBox { Header = "Mô tả phim", Text = movieDetail.Description ?? string.Empty, AcceptsReturn = true, Height = 60, Margin = new Thickness(0, 0, 0, 10) };
            var posterBox = new TextBox { Header = "Link Poster", Text = movieDetail.PosterUrl ?? string.Empty, Margin = new Thickness(0, 0, 0, 10) };
            var bannerBox = new TextBox { Header = "Link Banner", Text = movieDetail.BannerUrl ?? string.Empty, Margin = new Thickness(0, 0, 0, 10) };
            var trailerBox = new TextBox { Header = "Link Trailer", Text = string.Empty, Margin = new Thickness(0, 0, 0, 10) };
            var movieUrlBox = new TextBox { Header = "Link Phim (Stream)", Text = string.Empty, Margin = new Thickness(0, 0, 0, 10) };

            var yearBox = new NumberBox { Header = "Năm phát hành", Value = movieDetail.ReleaseYear, Margin = new Thickness(0, 0, 0, 10) };

            stackPanel.Children.Add(titleBox);
            stackPanel.Children.Add(descBox);
            stackPanel.Children.Add(posterBox);
            stackPanel.Children.Add(bannerBox);
            stackPanel.Children.Add(trailerBox);
            stackPanel.Children.Add(movieUrlBox);
            stackPanel.Children.Add(yearBox);

            var genreHeader = new TextBlock
            {
                Text = "Chọn thể loại",
                Margin = new Thickness(0, 10, 0, 5),
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
            };

            var genreContainer = new StackPanel { Margin = new Thickness(5, 0, 0, 15) };

            foreach (var genre in availableGenres)
            {
                var name = genre.Name ?? string.Empty;
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                var isChecked = selectedGenres.Contains(name);
                var cb = new CheckBox
                {
                    Content = name,
                    IsChecked = isChecked,
                    Margin = new Thickness(0, 0, 0, 5)
                };

                cb.Checked += (_, __) => selectedGenres.Add(name);
                cb.Unchecked += (_, __) => selectedGenres.Remove(name);

                genreContainer.Children.Add(cb);
            }

            stackPanel.Children.Add(genreHeader);
            stackPanel.Children.Add(genreContainer);

            var selectedCastList = new StackPanel { Margin = new Thickness(5, 5, 0, 15), Spacing = 4 };
            BuildCastAndCrewSection(stackPanel, selectedCastList, selectedCast, availableMembers);

            if (movieDetail.Actors != null)
            {
                foreach (var a in movieDetail.Actors)
                {
                    var member = availableMembers.FirstOrDefault(m => m.Id == a.MemberId);
                    if (member != null)
                    {
                        selectedCast.Add((member, a.Role));
                        AddCastRowToUI(selectedCastList, member, a.Role, selectedCast);
                    }
                }
            }

            var scrollViewer = new ScrollViewer
            {
                Content = stackPanel,
                MaxHeight = 600,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            var movieTitle = movieDetail.Title ?? $"#{movieId}";

            var dialog = new ContentDialog
            {
                Title = $"Chi tiết phim: {movieTitle}",
                Content = scrollViewer,
                PrimaryButtonText = "Lưu thay đổi",
                CloseButtonText = "Hủy",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = App.MainWindow.Content.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result != ContentDialogResult.Primary)
                return;

            var updatedTitle = titleBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(updatedTitle))
            {
                ShowInfo("Title không được để trống.", isError: true);
                return;
            }

            var reqBody = new MovieUpsertRequest
            {
                Title = updatedTitle,
                Description = descBox.Text,
                ReleaseYear = (int)yearBox.Value,
                PosterUrl = posterBox.Text.Trim(),
                BannerUrl = bannerBox.Text.Trim(),
                TrailerUrl = trailerBox.Text.Trim(),
                MovieUrl = movieUrlBox.Text.Trim(),
                Genres = selectedGenres
                    .Select(name => availableGenres.FirstOrDefault(g => string.Equals(g.Name, name, StringComparison.OrdinalIgnoreCase)))
                    .Where(g => g != null)
                    .Select(g => new GenreIdRequest { GenresId = g!.GenreId })
                    .DistinctBy(x => x.GenresId)
                    .ToList(),
                CastAndCrew = selectedCast
                    .Select(x => new CastAndCrewRequest
                    {
                        MemberId = x.Member.Id,
                        Role = x.Role
                    })
                    .ToList()
            };

            var put = new HttpRequestMessage(HttpMethod.Put, $"{BaseUrl}/api/movies/{movieId}")
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(reqBody, JsonLowerOptions),
                    Encoding.UTF8,
                    "application/json")
            };

            var res = await _http.SendAsync(put);
            if (!res.IsSuccessStatusCode)
            {
                var body = await res.Content.ReadAsStringAsync();
                ShowInfo($"Update fail: {(int)res.StatusCode} {body}", isError: true);
                return;
            }

            ShowInfo($"Đã cập nhật phim '{movieTitle}' thành công!", isError: false);
            await LoadMoviesAsync(resetPage: false);
        }
        catch (Exception ex)
        {
            ShowInfo($"Edit error: {ex.Message}", isError: true);
        }
    }

    [RelayCommand]
    private async Task AddMovieAsync()
    {
        try
        {
            SetAuthHeader();

            var availableGenres = Genres.ToList();
            var availableMembers = await LoadMembersAsync();

            var selectedGenres = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var selectedCast = new ObservableCollection<(MemberResult Member, string Role)>();

            var stackPanel = new StackPanel { Width = 500, Padding = new Thickness(0, 0, 15, 0) };

            var titleBox = new TextBox { Header = "Tiêu đề phim", Margin = new Thickness(0, 0, 0, 10) };
            var descBox = new TextBox
            {
                Header = "Mô tả phim",
                AcceptsReturn = true,
                Height = 60,
                Margin = new Thickness(0, 0, 0, 10)
            };
            var posterBox = new TextBox { Header = "Link Poster", Margin = new Thickness(0, 0, 0, 10) };
            var bannerBox = new TextBox { Header = "Link Banner", Margin = new Thickness(0, 0, 0, 10) };
            var trailerBox = new TextBox { Header = "Link Trailer", Margin = new Thickness(0, 0, 0, 10) };
            var movieUrlBox = new TextBox { Header = "Link Phim (Stream)", Margin = new Thickness(0, 0, 0, 10) };

            var yearBox = new NumberBox
            {
                Header = "Năm phát hành",
                Value = DateTime.Now.Year,
                Margin = new Thickness(0, 0, 0, 10)
            };

            stackPanel.Children.Add(titleBox);
            stackPanel.Children.Add(descBox);
            stackPanel.Children.Add(posterBox);
            stackPanel.Children.Add(bannerBox);
            stackPanel.Children.Add(trailerBox);
            stackPanel.Children.Add(movieUrlBox);
            stackPanel.Children.Add(yearBox);

            var genreHeader = new TextBlock
            {
                Text = "Chọn thể loại",
                Margin = new Thickness(0, 10, 0, 5),
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
            };

            var genreContainer = new StackPanel { Margin = new Thickness(5, 0, 0, 15) };

            foreach (var genre in availableGenres)
            {
                var name = genre.Name ?? string.Empty;
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                var cb = new CheckBox
                {
                    Content = name,
                    Margin = new Thickness(0, 0, 0, 5)
                };

                cb.Checked += (_, __) => selectedGenres.Add(name);
                cb.Unchecked += (_, __) => selectedGenres.Remove(name);

                genreContainer.Children.Add(cb);
            }

            stackPanel.Children.Add(genreHeader);
            stackPanel.Children.Add(genreContainer);

            var selectedCastList = new StackPanel { Margin = new Thickness(5, 5, 0, 15), Spacing = 4 };
            BuildCastAndCrewSection(stackPanel, selectedCastList, selectedCast, availableMembers);

            var scrollViewer = new ScrollViewer
            {
                Content = stackPanel,
                MaxHeight = 600,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            var dialog = new ContentDialog
            {
                Title = "Add New Movie",
                Content = scrollViewer,
                PrimaryButtonText = "Create",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = App.MainWindow.Content.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result != ContentDialogResult.Primary)
                return;

            var title = titleBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(title))
            {
                ShowInfo("Title không được để trống.", isError: true);
                return;
            }

            var reqBody = new MovieUpsertRequest
            {
                Title = title,
                Description = descBox.Text,
                ReleaseYear = (int)yearBox.Value,
                PosterUrl = posterBox.Text.Trim(),
                BannerUrl = bannerBox.Text.Trim(),
                TrailerUrl = trailerBox.Text.Trim(),
                MovieUrl = movieUrlBox.Text.Trim(),
                Genres = selectedGenres
                    .Select(name => availableGenres.FirstOrDefault(g => string.Equals(g.Name, name, StringComparison.OrdinalIgnoreCase)))
                    .Where(g => g != null)
                    .Select(g => new GenreIdRequest { GenresId = g!.GenreId })
                    .DistinctBy(x => x.GenresId)
                    .ToList(),
                CastAndCrew = selectedCast
                    .Select(x => new CastAndCrewRequest
                    {
                        MemberId = x.Member.Id,
                        Role = x.Role
                    })
                    .ToList()
            };

            var post = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/api/movies/")
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(reqBody, JsonLowerOptions),
                    Encoding.UTF8,
                    "application/json")
            };

            var res = await _http.SendAsync(post);
            if (!res.IsSuccessStatusCode)
            {
                var body = await res.Content.ReadAsStringAsync();
                ShowInfo($"Create fail: {(int)res.StatusCode} {body}", isError: true);
                return;
            }

            ShowInfo($"Tạo movie '{title}' thành công.", isError: false);
            await LoadMoviesAsync(resetPage: true);
        }
        catch (Exception ex)
        {
            ShowInfo($"Add movie error: {ex.Message}", isError: true);
        }
    }

    [RelayCommand]
    private async Task ImportExcelAsync()
    {
        try
        {
            var file = await PickExcelFileAsync();
            if (file == null)
                return;

            SetAuthHeader();

            IsImportVisible = true;
            ImportStatus = "Reading excel...";
            ImportSuccess = 0;
            ImportFailed = 0;
            ImportCurrent = 0;
            ImportTotal = 0;
            ImportProgress = 0;

            var rows = await Task.Run(() => ReadMoviesFromExcel(file.Path));
            if (rows.Count == 0)
            {
                ShowInfo("Excel không có dữ liệu movie.", isError: true);
                IsImportVisible = false;
                return;
            }

            ImportTotal = rows.Count;

            int ok = 0;
            int fail = 0;
            for (int i = 0; i < rows.Count; i++)
            {
                var r = rows[i];

                ImportCurrent = i + 1;
                ImportStatus = $"Importing: {r.Title}";
                ImportProgress = ((i + 1) * 100.0) / rows.Count;

                try
                {
                    var post = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/api/movies/")
                    {
                        Content = new StringContent(
                            JsonSerializer.Serialize(r, JsonLowerOptions),
                            Encoding.UTF8,
                            "application/json")
                    };

                    var res = await _http.SendAsync(post);

                    if (res.IsSuccessStatusCode)
                    {
                        ok++;
                        ImportSuccess = ok;
                    }
                    else
                    {
                        fail++;
                        ImportFailed = fail;
                    }
                }
                catch
                {
                    fail++;
                    ImportFailed = fail;
                }
            }

            ShowInfo($"Import xong. Success={ok}, Failed={fail}", isError: fail > 0);
            await LoadMoviesAsync(resetPage: true);
        }
        catch (Exception ex)
        {
            ShowInfo($"Import error: {ex.Message}", isError: true);
        }
    }

    private void BuildCastAndCrewSection(
        StackPanel stackPanel,
        StackPanel selectedCastList,
        ObservableCollection<(MemberResult Member, string Role)> targetCastCollection,
        List<MemberResult> availableMembers)
    {
        var castHeader = new TextBlock
        {
            Text = "Diễn viên & Đoàn làm phim",
            Margin = new Thickness(0, 10, 0, 5),
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
        };

        var memberCombo = new ComboBox
        {
            Header = "1. Chọn Nghệ sĩ",
            PlaceholderText = "Chọn nghệ sĩ...",
            ItemsSource = availableMembers,
            DisplayMemberPath = "Name",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Margin = new Thickness(0, 0, 0, 10)
        };

        var roleCombo = new ComboBox
        {
            Header = "2. Chọn Vai trò",
            ItemsSource = new[] { "Director", "Actor", "Producer" },
            SelectedIndex = 1,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Margin = new Thickness(0, 0, 0, 10)
        };

        var addButton = new Button
        {
            Content = "Thêm",
            Style = (Style)Application.Current.Resources["AccentButtonStyle"],
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 0, 0, 10)
        };

        addButton.Click += (_, __) =>
        {
            var selectedMember = memberCombo.SelectedItem as MemberResult;
            var selectedRole = roleCombo.SelectedItem as string;

            if (selectedMember != null && !string.IsNullOrEmpty(selectedRole))
            {
                targetCastCollection.Add((selectedMember, selectedRole));
                AddCastRowToUI(selectedCastList, selectedMember, selectedRole, targetCastCollection);
                memberCombo.SelectedIndex = -1;
            }
        };

        var selectedCastScrollViewer = new ScrollViewer
        {
            Content = selectedCastList,
            MaxHeight = 150,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };

        stackPanel.Children.Add(castHeader);
        stackPanel.Children.Add(memberCombo);
        stackPanel.Children.Add(roleCombo);
        stackPanel.Children.Add(addButton);
        stackPanel.Children.Add(selectedCastScrollViewer);
    }

    private void AddCastRowToUI(
        StackPanel selectedCastList,
        MemberResult member,
        string role,
        ObservableCollection<(MemberResult Member, string Role)> targetCastCollection)
    {
        var row = new Grid { Margin = new Thickness(0, 2, 0, 2) };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var roleBadge = new Border
        {
            Background = new SolidColorBrush(Colors.LightGray),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(6, 2, 6, 2),
            Margin = new Thickness(0, 0, 10, 0),
            Child = new TextBlock
            {
                Text = role,
                FontSize = 11,
                Foreground = new SolidColorBrush(Colors.Black),
                FontWeight = Microsoft.UI.Text.FontWeights.Bold
            }
        };

        var nameTxt = new TextBlock { Text = member.Name, VerticalAlignment = VerticalAlignment.Center };

        var removeBtn = new Button { Content = "X", FontSize = 10, Padding = new Thickness(5, 2, 5, 2) };
        removeBtn.Click += (_, __) =>
        {
            var itemToRemove = targetCastCollection.FirstOrDefault(x => x.Member.Id == member.Id && x.Role == role);
            if (itemToRemove.Member != null)
                targetCastCollection.Remove(itemToRemove);

            selectedCastList.Children.Remove(row);
        };

        row.Children.Add(roleBadge);
        row.Children.Add(nameTxt);
        row.Children.Add(removeBtn);

        Grid.SetColumn(nameTxt, 1);
        Grid.SetColumn(removeBtn, 2);

        selectedCastList.Children.Add(row);
    }

    private void SetAuthHeader()
    {
        var token = _sessionState.AccessToken?.Token;
        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException("Không lấy được token từ session. Hãy đăng nhập lại.");

        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private async Task LoadGenresAsync()
    {
        try
        {
            SetAuthHeader();

            var json = await _http.GetStringAsync($"{BaseUrl}/api/movies/genres");
            var dto = JsonSerializer.Deserialize<ApiListResponse<GenreDto>>(json, JsonInsensitiveOptions);

            Genres.Clear();
            if (dto?.Data != null)
            {
                foreach (var genre in dto.Data)
                {
                    Genres.Add(genre);
                }
            }

            SelectedGenre = null;
        }
        catch (Exception ex)
        {
            ShowInfo($"Load genres failed: {ex.Message}", isError: true);
        }
    }

    private async Task<int?> GetDirectorMemberIdFromDetailAsync(int movieId)
    {
        try
        {
            var json = await _http.GetStringAsync($"{BaseUrl}/api/movies/{movieId}");
            var detailRes = JsonSerializer.Deserialize<ApiSingleResponse<MovieDetailDto>>(json, JsonInsensitiveOptions);

            var directorId = detailRes?.Data?.Actors?
                .FirstOrDefault(x => string.Equals(x.Role, "Director", StringComparison.OrdinalIgnoreCase))
                ?.MemberId;

            return directorId;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GetDirectorMemberIdFromDetailAsync fail movieId={movieId}: {ex.Message}");
            return null;
        }
    }

    private async Task LoadMoviesAsync(bool resetPage)
    {
        try
        {
            if (resetPage)
                Page = 1;

            IsLoading = true;
            SetAuthHeader();

            var qs = new List<string>
            {
                $"page={Page}",
                $"limit={_limit}"
            };

            var title = SearchText?.Trim();
            if (!string.IsNullOrWhiteSpace(title))
                qs.Add($"search={Uri.EscapeDataString(title)}");

            if (int.TryParse(ReleaseYearText?.Trim(), out var year))
                qs.Add($"releaseYear={year}");

            if (SelectedGenre != null && !string.IsNullOrWhiteSpace(SelectedGenre.Name))
                qs.Add($"genre={Uri.EscapeDataString(SelectedGenre.Name)}");

            var url = $"{BaseUrl}/api/movies/?{string.Join("&", qs)}";

            var json = await _http.GetStringAsync(url);
            var dto = JsonSerializer.Deserialize<MoviesResponse>(json, JsonInsensitiveOptions);

            if (dto == null || dto.Success == false)
            {
                ShowInfo("API trả về không thành công.", isError: true);
                return;
            }

            var movies = dto.Data ?? new List<MovieDto>();

            var members = await LoadMembersAsync();
            var memberNameById = members.ToDictionary(x => x.Id, x => x.Name);

            var directorTasks = movies.Select(async m =>
            {
                var directorId = await GetDirectorMemberIdFromDetailAsync(m.MovieId);

                string directorName = string.Empty;
                if (directorId.HasValue && memberNameById.TryGetValue(directorId.Value, out var name))
                    directorName = name;

                return new { m.MovieId, DirectorName = directorName };
            }).ToList();

            var directors = await Task.WhenAll(directorTasks);
            var directorNameByMovieId = directors.ToDictionary(x => x.MovieId, x => x.DirectorName);

            Movies.Clear();
            foreach (var m in movies)
            {
                Movies.Add(new MovieRow
                {
                    MovieId = m.MovieId,
                    Title = m.Title ?? string.Empty,
                    ReleaseYear = m.ReleaseYear,
                    PosterUrl = m.PosterUrl,
                    GenresText = m.Genres != null ? string.Join(", ", m.Genres) : string.Empty,
                    DirectorName = directorNameByMovieId.TryGetValue(m.MovieId, out var dn) ? dn : string.Empty
                });
            }

            Page = dto.Pagination?.Page ?? Page;
            TotalPages = dto.Pagination?.TotalPages ?? 1;
        }
        catch (Exception ex)
        {
            ShowInfo($"Load movies failed: {ex.Message}", isError: true);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task<List<MemberResult>> LoadMembersAsync()
    {
        SetAuthHeader();
        var json = await _http.GetStringAsync($"{BaseUrl}/api/movies/members");
        var dto = JsonSerializer.Deserialize<ApiListResponse2<MemberResult>>(json, JsonInsensitiveOptions);
        return dto?.Data ?? new List<MemberResult>();
    }

    private static List<MovieUpsertRequest> ReadMoviesFromExcel(string path)
    {
        var result = new List<MovieUpsertRequest>();

        using var wb = new XLWorkbook(path);
        var ws = wb.Worksheets.First();

        var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
        for (int row = 2; row <= lastRow; row++)
        {
            var title = ws.Cell(row, 1).GetString()?.Trim();
            if (string.IsNullOrWhiteSpace(title))
                continue;

            var desc = ws.Cell(row, 2).GetString();
            var yearStr = ws.Cell(row, 3).GetString()?.Trim();
            int.TryParse(yearStr, out var year);

            var poster = ws.Cell(row, 4).GetString();
            var banner = ws.Cell(row, 5).GetString();

            result.Add(new MovieUpsertRequest
            {
                Title = title,
                Description = desc,
                ReleaseYear = year == 0 ? DateTime.Now.Year : year,
                PosterUrl = poster,
                BannerUrl = banner,
                Genres = new List<GenreIdRequest>(),
                CastAndCrew = new List<CastAndCrewRequest>()
            });
        }

        return result;
    }

    private static async Task<StorageFile?> PickExcelFileAsync()
    {
        var picker = new FileOpenPicker();
        picker.FileTypeFilter.Add(".xlsx");
        picker.FileTypeFilter.Add(".xls");

        var hwnd = WindowNative.GetWindowHandle(App.MainWindow);
        InitializeWithWindow.Initialize(picker, hwnd);

        return await picker.PickSingleFileAsync();
    }

    private void ShowInfo(string message, bool isError)
    {
        InfoTitle = isError ? "Error" : "Info";
        InfoMessage = message;
        InfoSeverity = isError ? InfoBarSeverity.Error : InfoBarSeverity.Success;
        IsInfoOpen = true;
    }

    public sealed class MoviesResponse
    {
        public bool Success { get; set; }
        public List<MovieDto>? Data { get; set; }
        public PaginationDto? Pagination { get; set; }
    }

    public sealed class MovieDto
    {
        public int MovieId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int ReleaseYear { get; set; }
        public string? PosterUrl { get; set; }
        public string? BannerUrl { get; set; }
        public double AvgRating { get; set; }
        public List<string>? Genres { get; set; }
    }

    public sealed class MemberResult
    {
        [JsonPropertyName("memberId")]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public sealed class ApiListResponse2<T>
    {
        public bool Success { get; set; }
        public List<T>? Data { get; set; }
    }

    public sealed class MovieActorDto
    {
        public int MemberId { get; set; }
        public string Role { get; set; } = string.Empty;
    }

    public sealed class MovieDetailDto
    {
        public int MovieId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int ReleaseYear { get; set; }
        public string? PosterUrl { get; set; }
        public string? BannerUrl { get; set; }
        public List<string>? Genres { get; set; }
        public List<MovieActorDto>? Actors { get; set; }
    }

    public sealed class PaginationDto
    {
        public int Page { get; set; }
        public int Limit { get; set; }
        public int Total { get; set; }
        public int TotalPages { get; set; }
    }

    public sealed class ApiListResponse<T>
    {
        public bool Success { get; set; }
        public List<T>? Data { get; set; }
    }

    public sealed class ApiSingleResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
    }

    public sealed class GenreDto
    {
        public int GenreId { get; set; }
        public string? Name { get; set; }
    }

    public sealed class MovieRow
    {
        public int MovieId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int ReleaseYear { get; set; }
        public string? PosterUrl { get; set; }
        public string GenresText { get; set; } = string.Empty;
        public string DirectorName { get; set; } = string.Empty;
    }

    public sealed class MovieUpsertRequest
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int ReleaseYear { get; set; }
        public string? PosterUrl { get; set; }
        public string? BannerUrl { get; set; }
        public string? TrailerUrl { get; set; }
        public string? MovieUrl { get; set; }
        public List<GenreIdRequest> Genres { get; set; } = new();
        public List<CastAndCrewRequest> CastAndCrew { get; set; } = new();
    }

    public sealed class GenreIdRequest
    {
        public int GenresId { get; set; }
    }

    public sealed class CastAndCrewRequest
    {
        [JsonPropertyName("memberId")]
        public int MemberId { get; set; }
        public string Role { get; set; } = string.Empty;
    }
}
