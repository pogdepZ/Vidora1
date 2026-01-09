// Excel
using ClosedXML.Excel;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Vidora.Core.Contracts.Services;
// File picker (WinUI 3)
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace Vidora.Presentation.Gui.Views
{
    public sealed partial class ManageMoviesPage : Page
    {
        private static readonly JsonSerializerOptions _jsonLowerOptions =
        new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
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

        private async Task<int?> GetDirectorMemberIdFromDetailAsync(int movieId)
        {
            try
            {
                var json = await _http.GetStringAsync($"{BaseUrl}/api/movies/{movieId}");
                var detailRes = JsonSerializer.Deserialize<ApiSingleResponse<MovieDetailDto>>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

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

                var movies = dto.Data ?? new List<MovieDto>();

                // load members 1 lần để map id -> name
                var members = await LoadMembersAsync();
                var memberNameById = members.ToDictionary(x => x.Id, x => x.Name);

                // gọi detail song song để lấy directorId
                var directorTasks = movies.Select(async m =>
                {
                    var directorId = await GetDirectorMemberIdFromDetailAsync(m.MovieId);

                    string directorName = "";
                    if (directorId.HasValue && memberNameById.TryGetValue(directorId.Value, out var name))
                        directorName = name;

                    return new { m.MovieId, DirectorName = directorName };
                }).ToList();

                var directors = await Task.WhenAll(directorTasks);
                var directorNameByMovieId = directors.ToDictionary(x => x.MovieId, x => x.DirectorName);

                // build rows
                var rows = movies.Select(m => new MovieRow
                {
                    MovieId = m.MovieId,
                    Title = m.Title ?? "",
                    ReleaseYear = m.ReleaseYear,
                    PosterUrl = m.PosterUrl,
                    GenresText = m.Genres != null ? string.Join(", ", m.Genres) : "",
                    DirectorName = directorNameByMovieId.TryGetValue(m.MovieId, out var dn) ? dn : ""
                }).ToList();

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

                // 1) Load movie detail
                var json = await _http.GetStringAsync($"{BaseUrl}/api/movies/{movieId}");
                var detailRes = JsonSerializer.Deserialize<ApiSingleResponse<MovieDetailDto>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // DEBUG: log raw json để xem field "actors" có về không
                System.Diagnostics.Debug.WriteLine("=== MOVIE DETAIL RAW JSON ===");
                System.Diagnostics.Debug.WriteLine(json);
                System.Diagnostics.Debug.WriteLine("=============================");

                System.Diagnostics.Debug.WriteLine($"Actors null? {detailRes?.Data?.Actors == null}");
                System.Diagnostics.Debug.WriteLine($"Actors count: {detailRes?.Data?.Actors?.Count ?? 0}");


                var movieDetail = detailRes?.Data;
                if (detailRes?.Success != true || movieDetail == null)
                {
                    ShowInfo("Không thể tải thông tin chi tiết phim.", isError: true);
                    return;
                }

                // 2) Load genres + members
                // genres bạn đã có GenreCombo.ItemsSource rồi, nhưng để chắc chắn ta lấy từ đó
                var availableGenres = (GenreCombo.ItemsSource as IEnumerable<GenreDto>)?.ToList()
                                     ?? new List<GenreDto>();

                var availableMembers = await LoadMembersAsync(); // endpoint tuỳ BE

                // 3) State edit
                var selectedGenres = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                if (movieDetail.Genres != null)
                    foreach (var g in movieDetail.Genres) selectedGenres.Add(g);

                var selectedCast = new System.Collections.ObjectModel.ObservableCollection<(MemberResult Member, string Role)>();

                // 4) Build UI dialog
                var stackPanel = new StackPanel { Width = 500, Padding = new Thickness(0, 0, 15, 0) };

                var titleBox = new TextBox { Header = "Tiêu đề phim", Text = movieDetail.Title ?? "", Margin = new Thickness(0, 0, 0, 10) };
                var descBox = new TextBox { Header = "Mô tả phim", Text = movieDetail.Description ?? "", AcceptsReturn = true, Height = 60, Margin = new Thickness(0, 0, 0, 10) };
                var posterBox = new TextBox { Header = "Link Poster", Text = movieDetail.PosterUrl ?? "", Margin = new Thickness(0, 0, 0, 10) };
                var bannerBox = new TextBox { Header = "Link Banner", Text = movieDetail.BannerUrl ?? "", Margin = new Thickness(0, 0, 0, 10) };

                // Nếu bạn chưa có TrailerUrl/MovieUrl trong DTO thì để trống hoặc thêm vào DTO
                var trailerBox = new TextBox { Header = "Link Trailer", Text = "", Margin = new Thickness(0, 0, 0, 10) };
                var movieUrlBox = new TextBox { Header = "Link Phim (Stream)", Text = "", Margin = new Thickness(0, 0, 0, 10) };

                var yearBox = new NumberBox { Header = "Năm phát hành", Value = movieDetail.ReleaseYear, Margin = new Thickness(0, 0, 0, 10) };

                stackPanel.Children.Add(titleBox);
                stackPanel.Children.Add(descBox);
                stackPanel.Children.Add(posterBox);
                stackPanel.Children.Add(bannerBox);
                stackPanel.Children.Add(trailerBox);
                stackPanel.Children.Add(movieUrlBox);
                stackPanel.Children.Add(yearBox);

                // ===== GENRES (checkbox pre-check) =====
                var genreHeader = new TextBlock
                {
                    Text = "Chọn thể loại",
                    Margin = new Thickness(0, 10, 0, 5),
                    FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
                };

                var genreContainer = new StackPanel { Margin = new Thickness(5, 0, 0, 15) };

                foreach (var genre in availableGenres)
                {
                    var name = genre.Name ?? "";
                    if (string.IsNullOrWhiteSpace(name)) continue;

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

                // ===== CAST / CREW =====
                var selectedCastList = new StackPanel { Margin = new Thickness(5, 5, 0, 15), Spacing = 4 };
                BuildCastAndCrewSection(stackPanel, selectedCastList, selectedCast, availableMembers);

                // Pre-populate existing cast
                if (movieDetail.Actors != null)
                {
                    System.Diagnostics.Debug.WriteLine("=== PREPOP CAST FROM API ===");
                    foreach (var a in movieDetail.Actors)
                    {
                        System.Diagnostics.Debug.WriteLine($"API actor -> memberId={a.MemberId}, role={a.Role}");

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
                    XamlRoot = this.XamlRoot
                };

                var result = await dialog.ShowAsync();
                if (result != ContentDialogResult.Primary) return;

                // 5) Build request body và gọi PUT
                var reqBody = new MovieUpsertRequest
                {
                    Title = titleBox.Text.Trim(),
                    Description = descBox.Text,
                    ReleaseYear = (int)yearBox.Value,
                    PosterUrl = posterBox.Text.Trim(),
                    BannerUrl = bannerBox.Text.Trim(),
                    TrailerUrl = trailerBox.Text.Trim(),
                    MovieUrl = movieUrlBox.Text.Trim(),

                    Genres = selectedGenres
        .Select(name => availableGenres.FirstOrDefault(g =>
            string.Equals(g.Name, name, StringComparison.OrdinalIgnoreCase)))
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
                    JsonSerializer.Serialize(reqBody, _jsonLowerOptions),
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

        private void BuildCastAndCrewSection(
    StackPanel stackPanel,
    StackPanel selectedCastList,
    System.Collections.ObjectModel.ObservableCollection<(MemberResult Member, string Role)> targetCastCollection,
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
                ItemsSource = new string[] { "Director", "Actor", "Producer" },
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

            addButton.Click += (s, arg) =>
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
            System.Collections.ObjectModel.ObservableCollection<(MemberResult Member, string Role)> targetCastCollection)
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


        private async void OnAddMovieButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                SetAuthHeader();

                // lấy list genres + members để build popup giống edit
                var availableGenres = (GenreCombo.ItemsSource as IEnumerable<GenreDto>)?.ToList()
                                     ?? new List<GenreDto>();

                var availableMembers = await LoadMembersAsync();

                // state
                var selectedGenres = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var selectedCast = new System.Collections.ObjectModel.ObservableCollection<(MemberResult Member, string Role)>();

                // UI
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

                // ===== GENRES (checkbox) =====
                var genreHeader = new TextBlock
                {
                    Text = "Chọn thể loại",
                    Margin = new Thickness(0, 10, 0, 5),
                    FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
                };

                var genreContainer = new StackPanel { Margin = new Thickness(5, 0, 0, 15) };

                foreach (var genre in availableGenres)
                {
                    var name = genre.Name ?? "";
                    if (string.IsNullOrWhiteSpace(name)) continue;

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

                // ===== CAST / CREW =====
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
                    XamlRoot = this.XamlRoot
                };

                var result = await dialog.ShowAsync();
                if (result != ContentDialogResult.Primary) return;

                // validate
                var title = titleBox.Text.Trim();
                if (string.IsNullOrWhiteSpace(title))
                {
                    ShowInfo("Title không được để trống.", isError: true);
                    return;
                }

                // build body theo mẫu
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
                        .Select(name => availableGenres.FirstOrDefault(g =>
                            string.Equals(g.Name, name, StringComparison.OrdinalIgnoreCase)))
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
                        JsonSerializer.Serialize(reqBody, _jsonLowerOptions),
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
                                JsonSerializer.Serialize(r, _jsonLowerOptions),
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
                    Genres = new List<GenreIdRequest>(),
                    CastAndCrew = new List<CastAndCrewRequest>()
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

        private async Task<List<MemberResult>> LoadMembersAsync()
        {
            SetAuthHeader();
            var json = await _http.GetStringAsync($"{BaseUrl}/api/movies/members"); // <-- đổi theo BE
            var dto = JsonSerializer.Deserialize<ApiListResponse2<MemberResult>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            System.Diagnostics.Debug.WriteLine("=== MEMBERS RAW JSON ===");
            System.Diagnostics.Debug.WriteLine(json);
            System.Diagnostics.Debug.WriteLine("========================");
            return dto?.Data ?? new List<MemberResult>();
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
            public string Name { get; set; } = "";
        }

        public sealed class ApiListResponse2<T>
        {
            public bool Success { get; set; }
            public List<T>? Data { get; set; }
        }


        public sealed class MovieActorDto
        {
            public int MemberId { get; set; }
            public string Role { get; set; } = ""; // "Director"/"Actor"/"Producer"
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
            public string? TrailerUrl { get; set; }
            public string? MovieUrl { get; set; }

            // genres: [{ "genresId": 1 }, { "genresId": 2 }]
            public List<GenreIdRequest> Genres { get; set; } = new();

            // castAndCrew: [{ "memberId": 1, "role": "Actor" }, ...]
            public List<CastAndCrewRequest> CastAndCrew { get; set; } = new();
        }

        public sealed class GenreIdRequest
        {
            // đúng theo mẫu: "genresId"
            public int GenresId { get; set; }
        }

        public sealed class CastAndCrewRequest
        {
            [JsonPropertyName("memberId")]
            public int MemberId { get; set; }
            public string Role { get; set; } = "";
        }
    }
}
