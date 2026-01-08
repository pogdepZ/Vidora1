using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Vidora.Core.Contracts.Services;

namespace Vidora.Presentation.Gui.Views;

public sealed partial class ManageUsersPage : Page
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ISessionStateService _session;

    private int _page = 1;
    private int _limit = 10;
    private int _totalPages = 1;
    private int _total = 0;

    private string _search = "";
    private string _status = "";

    public ManageUsersPage()
    {
        InitializeComponent();

        _httpClientFactory = App.GetService<IHttpClientFactory>();
        _session = App.GetService<ISessionStateService>();

        StatusCombo.Items.Add("All");
        StatusCombo.Items.Add("ACTIVE");
        StatusCombo.Items.Add("LOCKED");
        StatusCombo.SelectedIndex = 0;

        Loaded += async (_, __) => await LoadUsersAsync(resetPage: true);
    }

    // ========= DTOs (match API JSON) =========
    private sealed class UsersListResponse
    {
        public bool Success { get; set; }
        public List<UserItem>? Data { get; set; }
        public PaginationDto? Pagination { get; set; }
        public string? Message { get; set; }
    }

    private sealed class PaginationDto
    {
        public int Page { get; set; }
        public int Limit { get; set; }
        public int Total { get; set; }
        public int TotalPages { get; set; }
    }

    private sealed class UserItem
    {
        public int UserId { get; set; }
        public string? FullName { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Avatar { get; set; }
        public string? Role { get; set; }
        public string? Status { get; set; }
        public string? CreatedAt { get; set; }
        public string? Gender { get; set; }
        public string? Birthday { get; set; }

        // for UI
        public string CreatedAtDisplay { get; set; } = "";
    }

    private sealed class UserDetailResponse
    {
        public bool Success { get; set; }
        public UserDetailData? Data { get; set; }
        public string? Message { get; set; }
    }

    private sealed class UserDetailData
    {
        public UserItem? User { get; set; }
        public object[]? Subscriptions { get; set; }
        public List<OrderDto>? Orders { get; set; }
        public object[]? History { get; set; }
    }

    private sealed class OrderDto
    {
        public int OrderId { get; set; }
        public string? OrderCode { get; set; }
        public string? Status { get; set; }
        public int Amount { get; set; }
        public string? PaidAt { get; set; }
        public string? PlanName { get; set; }
        public int PlanPrice { get; set; }
        public int Durations { get; set; }
        public int FinalAmount { get; set; }
    }

    private sealed class ErrorBody
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
    }

    // ========= Helpers =========
    private HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient("ApiClient"); // nếu bạn đã đặt tên khác thì đổi ở đây

        // Nếu bạn không đặt named client, dùng CreateClient() thường:
        // var client = _httpClientFactory.CreateClient();

        var token = _session.CurrentSession?.AccessToken?.Token;
        if (!string.IsNullOrWhiteSpace(token))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
        return client;
    }

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private void SetLoading(bool isLoading)
    {
        LoadingRing.IsActive = isLoading;
        LoadingRing.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
    }

    private void ShowInfo(string msg, InfoBarSeverity severity)
    {
        NotificationInfoBar.Severity = severity;
        NotificationInfoBar.Title = severity == InfoBarSeverity.Success ? "Success" : "Error";
        NotificationInfoBar.Message = msg;
        NotificationInfoBar.IsOpen = true;
    }

    private static string FormatIsoDate(string? iso)
    {
        if (string.IsNullOrWhiteSpace(iso)) return "";
        if (DateTimeOffset.TryParse(iso, out var dt))
        {
            // format gọn: 2026-01-02
            return dt.ToLocalTime().ToString("yyyy-MM-dd");
        }
        return iso!;
    }

    private void UpdatePagingUI()
    {
        PageRun.Text = _page.ToString();
        TotalPagesRun.Text = _totalPages.ToString();
        TotalUsersRun.Text = _total.ToString();

        PrevBtn.IsEnabled = _page > 1;
        NextBtn.IsEnabled = _page < _totalPages;
    }

    // ========= API Calls =========
    private async Task LoadUsersAsync(bool resetPage)
    {
        if (resetPage) _page = 1;

        _search = SearchBox.Text?.Trim() ?? "";
        var statusSelected = StatusCombo.SelectedItem?.ToString() ?? "All";
        _status = statusSelected == "All" ? "" : statusSelected;

        try
        {
            SetLoading(true);

            var client = CreateClient();

            // query: /api/users/?page=1&limit=10&search=&status=
            var url = new StringBuilder();
            url.Append($"/api/users/?page={_page}&limit={_limit}");

            if (!string.IsNullOrWhiteSpace(_search))
                url.Append($"&search={Uri.EscapeDataString(_search)}");

            if (!string.IsNullOrWhiteSpace(_status))
                url.Append($"&status={Uri.EscapeDataString(_status)}");

            var res = await client.GetAsync(url.ToString());

            if (res.IsSuccessStatusCode)
            {
                var json = await res.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<UsersListResponse>(json, JsonOpts);

                if (data?.Data == null)
                {
                    UsersList.ItemsSource = null;
                    _totalPages = 1;
                    _total = 0;
                    UpdatePagingUI();
                    return;
                }

                foreach (var u in data.Data)
                {
                    u.FullName ??= "";
                    u.Username ??= "";
                    u.Email ??= "";
                    u.Role ??= "";
                    u.Status ??= "";
                    u.CreatedAtDisplay = FormatIsoDate(u.CreatedAt);
                }

                UsersList.ItemsSource = data.Data;

                _totalPages = data.Pagination?.TotalPages ?? 1;
                _total = data.Pagination?.Total ?? data.Data.Count;

                UpdatePagingUI();
                return;
            }

            // lỗi
            var errJson = await res.Content.ReadAsStringAsync();
            var err = JsonSerializer.Deserialize<ErrorBody>(errJson, JsonOpts);
            ShowInfo(err?.Message ?? $"Load users failed ({(int)res.StatusCode})", InfoBarSeverity.Error);
        }
        catch (Exception ex)
        {
            ShowInfo(ex.Message, InfoBarSeverity.Error);
        }
        finally
        {
            SetLoading(false);
        }
    }

    private async Task ShowUserDetailAsync(int userId)
    {
        try
        {
            SetLoading(true);

            var client = CreateClient();
            var res = await client.GetAsync($"/api/users/{userId}");

            if (res.IsSuccessStatusCode)
            {
                var json = await res.Content.ReadAsStringAsync();
                var dto = JsonSerializer.Deserialize<UserDetailResponse>(json, JsonOpts);

                if (dto?.Data?.User == null)
                {
                    ShowInfo("No detail data.", InfoBarSeverity.Error);
                    return;
                }

                var u = dto.Data.User;
                var ordersCount = dto.Data.Orders?.Count ?? 0;

                var content = new StringBuilder();
                content.AppendLine($"FullName: {u.FullName}");
                content.AppendLine($"Username: {u.Username}");
                content.AppendLine($"Email: {u.Email}");
                content.AppendLine($"Role: {u.Role}");
                content.AppendLine($"Status: {u.Status}");
                content.AppendLine($"CreatedAt: {FormatIsoDate(u.CreatedAt)}");
                content.AppendLine($"Orders: {ordersCount}");

                var dialog = new ContentDialog
                {
                    Title = $"User Detail (ID: {u.UserId})",
                    Content = new TextBlock { Text = content.ToString(), TextWrapping = TextWrapping.Wrap },
                    CloseButtonText = "Close",
                    XamlRoot = this.XamlRoot
                };

                await dialog.ShowAsync();
                return;
            }

            var errJson = await res.Content.ReadAsStringAsync();
            var err = JsonSerializer.Deserialize<ErrorBody>(errJson, JsonOpts);
            ShowInfo(err?.Message ?? $"Get detail failed ({(int)res.StatusCode})", InfoBarSeverity.Error);
        }
        catch (Exception ex)
        {
            ShowInfo(ex.Message, InfoBarSeverity.Error);
        }
        finally
        {
            SetLoading(false);
        }
    }

    private async Task ToggleUserStatusAsync(int userId)
    {
        try
        {
            SetLoading(true);

            var client = CreateClient();

            // Bạn chưa gửi body chuẩn, nên mình gửi {}.
            // Nếu BE yêu cầu: { "status": "LOCKED" } hoặc gì đó -> bạn đổi dòng bodyJson.
            var bodyJson = "{}";
            var content = new StringContent(bodyJson, Encoding.UTF8, "application/json");

            var res = await client.PutAsync($"/api/users/{userId}/status", content);

            if (res.IsSuccessStatusCode)
            {
                ShowInfo("Update status success.", InfoBarSeverity.Success);
                await LoadUsersAsync(resetPage: false);
                return;
            }

            var errJson = await res.Content.ReadAsStringAsync();
            var err = JsonSerializer.Deserialize<ErrorBody>(errJson, JsonOpts);
            ShowInfo(err?.Message ?? $"Update status failed ({(int)res.StatusCode})", InfoBarSeverity.Error);
        }
        catch (Exception ex)
        {
            ShowInfo(ex.Message, InfoBarSeverity.Error);
        }
        finally
        {
            SetLoading(false);
        }
    }

    // ========= UI Events =========
    private async void Reload_Click(object sender, RoutedEventArgs e)
        => await LoadUsersAsync(resetPage: false);

    private async void Search_Click(object sender, RoutedEventArgs e)
        => await LoadUsersAsync(resetPage: true);

    private async void Clear_Click(object sender, RoutedEventArgs e)
    {
        SearchBox.Text = "";
        StatusCombo.SelectedIndex = 0;
        await LoadUsersAsync(resetPage: true);
    }

    private async void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        => await LoadUsersAsync(resetPage: true);

    private async void Prev_Click(object sender, RoutedEventArgs e)
    {
        if (_page <= 1) return;
        _page--;
        await LoadUsersAsync(resetPage: false);
    }

    private async void Next_Click(object sender, RoutedEventArgs e)
    {
        if (_page >= _totalPages) return;
        _page++;
        await LoadUsersAsync(resetPage: false);
    }

    private async void ViewDetail_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is int userId)
        {
            await ShowUserDetailAsync(userId);
        }
        else if (sender is Button btn2 && int.TryParse(btn2.Tag?.ToString(), out var id))
        {
            await ShowUserDetailAsync(id);
        }
    }

    private async void ToggleStatus_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is int userId)
        {
            await ToggleUserStatusAsync(userId);
        }
        else if (sender is Button btn2 && int.TryParse(btn2.Tag?.ToString(), out var id))
        {
            await ToggleUserStatusAsync(id);
        }
    }
}
