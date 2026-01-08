using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Vidora.Core.Contracts.Services;

// Excel
using ClosedXML.Excel;

// File picker (WinUI 3)
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace Vidora.Presentation.Gui.Views
{
    public sealed partial class ManageMoviesPage : Page
    {
        private readonly ISessionStateService _sessionState = App.GetService<ISessionStateService>();
        private static readonly HttpClient _http = new HttpClient();

        private const string BaseUrl = "http://localhost:3000";

        private int _page = 1;
        private int _limit = 10;
        private int _totalPages = 1;

        public ManageMoviesPage()
        {
            InitializeComponent();
            Loaded += ManageMoviesPage_Loaded;
        }

        private async void ManageMoviesPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadGenresAsync();
            await LoadMoviesAsync(resetPage: true);
        }

        // =========================
        // AUTH
        // =========================
        private void SetAuthHeader()
        {
            var token = _sessionState.AccessToken?.Token;
            if (string.IsNullOrWhiteSpace(token))
                throw new InvalidOperationException("Không lấy được token từ session. Hãy đăng nhập lại.");

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        // =========================
        // API LOAD
        // =========================
        private async Task LoadGenresAsync()
        {
            try
            {
                SetAuthHeader();

                var json = await _http.GetStringAsync($"{BaseUrl}/api/movies/genres");
                var dto = JsonSerializer.Deserialize<ApiListResponse<GenreDto>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                GenreCombo.ItemsSource = dto?.Data ?? new List<GenreDto>();
                GenreCombo.SelectedItem = null;
            }
            catch (Exception ex)
            {
                ShowInfo($"Load genres failed: {ex.Message}", isError: true);
            }
        }

        private async Task LoadMoviesAsync(bool resetPage)
        {
            try
            {
                if (resetPage) _page = 1;

                LoadingRing.IsActive = true;
                LoadingRing.Visibility = Visibility.Visible;

                SetAuthHeader();

                // Backend bạn nói "BE có hỗ trợ" -> mình gửi query params thường gặp.
                // Nếu backend bạn đặt tên khác, đổi 3 param: search, releaseYear, genre
                var qs = new List<string>
                {
                    $"page={_page}",
                    $"limit={_limit}",
                };

                var title = SearchBox.Text?.Trim();
                if (!string.IsNullOrWhiteSpace(title))
                    qs.Add($"search={Uri.EscapeDataString(title)}");

                if (int.TryParse(ReleaseYearBox.Text?.Trim(), out var year))
                    qs.Add($"releaseYear={year}");

                if (GenreCombo.SelectedItem is GenreDto g && !string.IsNullOrWhiteSpace(g.Name))
                    qs.Add($"genre={Uri.EscapeDataString(g.Name)}");

                var url = $"{BaseUrl}/api/movies/?{string.Join("&", qs)}";

                var json = await _http.GetStringAsync(url);
                var dto = JsonSerializer.Deserialize<MoviesResponse>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (dto == null || dto.Success == false)
                {
                    ShowInfo("API trả về không thành công.", isError: true);
                    return;
                }

                var rows = (dto.Data ?? new List<MovieDto>())
                    .Select(m => new MovieRow
                    {
                        MovieId = m.MovieId,
                        Title = m.Title ?? "",
                        ReleaseYear = m.ReleaseYear,
                        PosterUrl = m.PosterUrl,
                        GenresText = m.Genres != null ? string.Join(", ", m.Genres) : "",
                        DirectorName = "" // list API hiện chưa có director
                    })
                    .ToList();

                MoviesList.ItemsSource = rows;

                _page = dto.Pagination?.Page ?? _page;
                _totalPages = dto.Pagination?.TotalPages ?? 1;

                CurrentPageRun.Text = _page.ToString();
                TotalPagesRun.Text = _totalPages.ToString();

                PrevBtn.IsEnabled = _page > 1;
                NextBtn.IsEnabled = _page < _totalPages;
            }
            catch (Exception ex)
            {
                ShowInfo($"Load movies failed: {ex.Message}", isError: true);
            }
            finally
            {
                LoadingRing.IsActive = false;
                LoadingRing.Visibility = Visibility.Collapsed;
            }
        }

        // =========================
        // UI EVENTS: FILTER/PAGING
        // =========================
        private async void OnSearchClick(object sender, RoutedEventArgs e)
            => await LoadMoviesAsync(resetPage: true);

        private async void OnClearClick(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = "";
            ReleaseYearBox.Text = "";
            GenreCombo.SelectedItem = null;
            await LoadMoviesAsync(resetPage: true);
        }

        private async void OnSearchQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
            => await LoadMoviesAsync(resetPage: true);

        private async void OnPrevClick(object sender, RoutedEventArgs e)
        {
            if (_page <= 1) return;
            _page--;
            await LoadMoviesAsync(resetPage: false);
        }

        private async void OnNextClick(object sender, RoutedEventArgs e)
        {
            if (_page >= _totalPages) return;
            _page++;
            await LoadMoviesAsync(resetPage: false);
        }

        // =========================
        // DELETE / TOGGLE DELETE
        // =========================
        private async void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            if (btn.Tag is not int movieId) return;

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

        private async void OnViewEditClick(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            if (btn.Tag is not int movieId) return;

            try
            {
                SetAuthHeader();

                // GET detail
                var json = await _http.GetStringAsync($"{BaseUrl}/api/movies/{movieId}");
                var detail = JsonSerializer.Deserialize<ApiSingleResponse<MovieDetailDto>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (detail?.Success != true || detail.Data == null)
                {
                    ShowInfo("Không lấy được movie detail.", isError: true);
                    return;
                }

                var m = detail.Data;

                // Dialog UI
                var titleBox = new TextBox { Text = m.Title ?? "", PlaceholderText = "Title" };
                var yearBox = new TextBox { Text = m.ReleaseYear.ToString(), PlaceholderText = "ReleaseYear" };
                var posterBox = new TextBox { Text = m.PosterUrl ?? "", PlaceholderText = "PosterUrl" };
                var bannerBox = new TextBox { Text = m.BannerUrl ?? "", PlaceholderText = "BannerUrl" };
                var descBox = new TextBox
                {
                    Text = m.Description ?? "",
                    PlaceholderText = "Description",
                    AcceptsReturn = true,
                    TextWrapping = TextWrapping.Wrap,
                    Height = 120
                };
                var genresBox = new TextBox
                {
                    Text = m.Genres != null ? string.Join(", ", m.Genres) : "",
                    PlaceholderText = "Genres (comma separated)"
                };

                var panel = new StackPanel { Spacing = 10 };
                panel.Children.Add(titleBox);
                panel.Children.Add(yearBox);
                panel.Children.Add(posterBox);
                panel.Children.Add(bannerBox);
                panel.Children.Add(genresBox);
                panel.Children.Add(descBox);

                var dialog = new ContentDialog
                {
                    Title = $"Edit Movie #{movieId}",
                    Content = panel,
                    PrimaryButtonText = "Save",
                    CloseButtonText = "Cancel",
                    XamlRoot = this.XamlRoot
                };

                var result = await dialog.ShowAsync();
                if (result != ContentDialogResult.Primary) return;

                if (!int.TryParse(yearBox.Text.Trim(), out var year))
                {
                    ShowInfo("ReleaseYear không hợp lệ.", isError: true);
                    return;
                }

                var reqBody = new MovieUpsertRequest
                {
                    Title = titleBox.Text.Trim(),
                    Description = descBox.Text,
                    ReleaseYear = year,
                    PosterUrl = posterBox.Text.Trim(),
                    BannerUrl = bannerBox.Text.Trim(),
                    Genres = SplitGenres(genresBox.Text)
                };

                var put = new HttpRequestMessage(HttpMethod.Put, $"{BaseUrl}/api/movies/{movieId}")
                {
                    Content = new StringContent(
                        JsonSerializer.Serialize(reqBody),
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

                ShowInfo("Update movie thành công.", isError: false);
                await LoadMoviesAsync(resetPage: false);
            }
            catch (Exception ex)
            {
                ShowInfo($"Edit error: {ex.Message}", isError: true);
            }
        }

        private async void OnAddMovieButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var titleBox = new TextBox { PlaceholderText = "Title" };
                var yearBox = new TextBox { PlaceholderText = "ReleaseYear" };
                var posterBox = new TextBox { PlaceholderText = "PosterUrl" };
                var bannerBox = new TextBox { PlaceholderText = "BannerUrl" };
                var genresBox = new TextBox { PlaceholderText = "Genres (comma separated)" };
                var descBox = new TextBox
                {
                    PlaceholderText = "Description",
                    AcceptsReturn = true,
                    TextWrapping = TextWrapping.Wrap,
                    Height = 120
                };

                var panel = new StackPanel { Spacing = 10 };
                panel.Children.Add(titleBox);
                panel.Children.Add(yearBox);
                panel.Children.Add(posterBox);
                panel.Children.Add(bannerBox);
                panel.Children.Add(genresBox);
                panel.Children.Add(descBox);

                var dialog = new ContentDialog
                {
                    Title = "Add New Movie",
                    Content = panel,
                    PrimaryButtonText = "Create",
                    CloseButtonText = "Cancel",
                    XamlRoot = this.XamlRoot
                };

                var result = await dialog.ShowAsync();
                if (result != ContentDialogResult.Primary) return;

                if (!int.TryParse(yearBox.Text.Trim(), out var year))
                {
                    ShowInfo("ReleaseYear không hợp lệ.", isError: true);
                    return;
                }

                SetAuthHeader();

                var reqBody = new MovieUpsertRequest
                {
                    Title = titleBox.Text.Trim(),
                    Description = descBox.Text,
                    ReleaseYear = year,
                    PosterUrl = posterBox.Text.Trim(),
                    BannerUrl = bannerBox.Text.Trim(),
                    Genres = SplitGenres(genresBox.Text)
                };

                var post = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/api/movies/")
                {
                    Content = new StringContent(
                        JsonSerializer.Serialize(reqBody),
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

                ShowInfo("Tạo movie thành công.", isError: false);
                await LoadMoviesAsync(resetPage: true);
            }
            catch (Exception ex)
            {
                ShowInfo($"Add movie error: {ex.Message}", isError: true);
            }
        }
        private async void OnImportExcelButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var file = await PickExcelFileAsync();
                if (file == null) return;

                SetAuthHeader();

                ImportSection.Visibility = Visibility.Visible;
                ImportStatusText.Text = "Reading excel...";
                ImportSuccessText.Text = "0";
                ImportFailedText.Text = "0";
                ImportCurrentRun.Text = "0";
                ImportTotalRun.Text = "0";
                ImportProgressBar.Value = 0;

                // Read rows from excel
                var rows = await Task.Run(() => ReadMoviesFromExcel(file.Path));
                if (rows.Count == 0)
                {
                    ShowInfo("Excel không có dữ liệu movie.", isError: true);
                    ImportSection.Visibility = Visibility.Collapsed;
                    return;
                }

                ImportTotalRun.Text = rows.Count.ToString();

                int ok = 0, fail = 0;
                for (int i = 0; i < rows.Count; i++)
                {
                    var r = rows[i];

                    ImportCurrentRun.Text = (i + 1).ToString();
                    ImportStatusText.Text = $"Importing: {r.Title}";
                    ImportProgressBar.Value = ((i + 1) * 100.0) / rows.Count;

                    try
                    {
                        var post = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/api/movies/")
                        {
                            Content = new StringContent(
                                JsonSerializer.Serialize(r),
                                Encoding.UTF8,
                                "application/json")
                        };

                        var res = await _http.SendAsync(post);

                        if (res.IsSuccessStatusCode)
                        {
                            ok++;
                            ImportSuccessText.Text = ok.ToString();
                        }
                        else
                        {
                            fail++;
                            ImportFailedText.Text = fail.ToString();
                        }
                    }
                    catch
                    {
                        fail++;
                        ImportFailedText.Text = fail.ToString();
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
        private List<MovieUpsertRequest> ReadMoviesFromExcel(string path)
        {
            var result = new List<MovieUpsertRequest>();

            using var wb = new XLWorkbook(path);
            var ws = wb.Worksheets.First();

            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
            for (int row = 2; row <= lastRow; row++)
            {
                var title = ws.Cell(row, 1).GetString()?.Trim();
                if (string.IsNullOrWhiteSpace(title)) continue;

                var desc = ws.Cell(row, 2).GetString();
                var yearStr = ws.Cell(row, 3).GetString()?.Trim();
                int.TryParse(yearStr, out var year);

                var poster = ws.Cell(row, 4).GetString();
                var banner = ws.Cell(row, 5).GetString();
                var genres = ws.Cell(row, 6).GetString();

                result.Add(new MovieUpsertRequest
                {
                    Title = title,
                    Description = desc,
                    ReleaseYear = year == 0 ? DateTime.Now.Year : year,
                    PosterUrl = poster,
                    BannerUrl = banner,
                    Genres = SplitGenres(genres)
                });
            }

            return result;
        }

        private async Task<StorageFile?> PickExcelFileAsync()
        {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".xlsx");
            picker.FileTypeFilter.Add(".xls");

            // WinUI 3 cần init với window handle
            var hwnd = WindowNative.GetWindowHandle(App.MainWindow);
            InitializeWithWindow.Initialize(picker, hwnd);

            return await picker.PickSingleFileAsync();
        }

        private List<string> SplitGenres(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return new List<string>();
            return raw.Split(',', StringSplitOptions.RemoveEmptyEntries)
                      .Select(x => x.Trim())
                      .Where(x => !string.IsNullOrWhiteSpace(x))
                      .Distinct(StringComparer.OrdinalIgnoreCase)
                      .ToList();
        }

        private void ShowInfo(string message, bool isError)
        {
            NotificationInfoBar.Title = isError ? "Error" : "Info";
            NotificationInfoBar.Message = message;
            NotificationInfoBar.Severity = isError ? InfoBarSeverity.Error : InfoBarSeverity.Success;
            NotificationInfoBar.IsOpen = true;
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

        public sealed class MovieDetailDto
        {
            public int MovieId { get; set; }
            public string? Title { get; set; }
            public string? Description { get; set; }
            public int ReleaseYear { get; set; }
            public string? PosterUrl { get; set; }
            public string? BannerUrl { get; set; }
            public List<string>? Genres { get; set; }
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
            public string Title { get; set; } = "";
            public int ReleaseYear { get; set; }
            public string? PosterUrl { get; set; }
            public string GenresText { get; set; } = "";
            public string DirectorName { get; set; } = "";
        }

     
        public sealed class MovieUpsertRequest
        {
            public string Title { get; set; } = "";
            public string? Description { get; set; }
            public int ReleaseYear { get; set; }
            public string? PosterUrl { get; set; }
            public string? BannerUrl { get; set; }
            public List<string> Genres { get; set; } = new();
        }
    }
}
