using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Vidora.Core.Contracts.Services;
using Vidora.Presentation.Gui.Contracts.ViewModels;

namespace Vidora.Presentation.Gui.ViewModels;

public partial class ManageUsersViewModel : ObservableRecipient, INavigationAware
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ISessionStateService _session;

    private readonly int _limit = 10;

    public ObservableCollection<UserItem> Users { get; } = new();
    public ObservableCollection<string> StatusOptions { get; } = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _selectedStatus = "All";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanPrev))]
    [NotifyPropertyChangedFor(nameof(CanNext))]
    [NotifyPropertyChangedFor(nameof(PageText))]
    private int _page = 1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanNext))]
    [NotifyPropertyChangedFor(nameof(TotalPagesText))]
    private int _totalPages = 1;

    [ObservableProperty]
    private int _totalUsers;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isInfoOpen;

    [ObservableProperty]
    private string _infoMessage = string.Empty;

    [ObservableProperty]
    private string _infoTitle = "Success";

    [ObservableProperty]
    private InfoBarSeverity _infoSeverity = InfoBarSeverity.Success;

    public bool CanPrev => Page > 1;
    public bool CanNext => Page < TotalPages;
    public string PageText => Page.ToString();
    public string TotalPagesText => TotalPages.ToString();

    public ManageUsersViewModel(IHttpClientFactory httpClientFactory, ISessionStateService session)
    {
        _httpClientFactory = httpClientFactory;
        _session = session;

        StatusOptions.Add("All");
        StatusOptions.Add("ACTIVE");
        StatusOptions.Add("LOCKED");
    }

    public async Task OnNavigatedToAsync(object parameter)
    {
        await LoadUsersAsync(resetPage: true);
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
    private async Task ReloadAsync()
        => await LoadUsersAsync(resetPage: false);

    [RelayCommand]
    private async Task SearchAsync()
        => await LoadUsersAsync(resetPage: true);

    [RelayCommand]
    private async Task ClearAsync()
    {
        SearchText = string.Empty;
        SelectedStatus = StatusOptions.Count > 0 ? StatusOptions[0] : "All";
        await LoadUsersAsync(resetPage: true);
    }

    [RelayCommand]
    private async Task PrevAsync()
    {
        if (Page <= 1)
            return;

        Page--;
        await LoadUsersAsync(resetPage: false);
    }

    [RelayCommand]
    private async Task NextAsync()
    {
        if (Page >= TotalPages)
            return;

        Page++;
        await LoadUsersAsync(resetPage: false);
    }

    [RelayCommand]
    private async Task ViewDetailAsync(int userId)
    {
        await ShowUserDetailAsync(userId);
    }

    [RelayCommand]
    private async Task ToggleStatusAsync(int userId)
    {
        await ToggleUserStatusAsync(userId);
    }

    private HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient("ApiClient");

        var token = _session.CurrentSession?.AccessToken?.Token;
        if (!string.IsNullOrWhiteSpace(token))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
        return client;
    }

    private void ShowInfo(string msg, InfoBarSeverity severity)
    {
        InfoSeverity = severity;
        InfoTitle = severity == InfoBarSeverity.Success ? "Success" : "Error";
        InfoMessage = msg;
        IsInfoOpen = true;
    }

    private static string FormatIsoDate(string? iso)
    {
        if (string.IsNullOrWhiteSpace(iso))
            return string.Empty;

        if (DateTimeOffset.TryParse(iso, out var dt))
        {
            return dt.ToLocalTime().ToString("yyyy-MM-dd");
        }

        return iso;
    }

    private async Task LoadUsersAsync(bool resetPage)
    {
        if (resetPage)
            Page = 1;

        var search = SearchText?.Trim() ?? string.Empty;
        var statusSelected = SelectedStatus ?? "All";
        var status = statusSelected == "All" ? string.Empty : statusSelected;

        try
        {
            IsLoading = true;

            var client = CreateClient();

            var url = new StringBuilder();
            url.Append($"/api/users/?page={Page}&limit={_limit}");

            if (!string.IsNullOrWhiteSpace(search))
                url.Append($"&search={Uri.EscapeDataString(search)}");

            if (!string.IsNullOrWhiteSpace(status))
                url.Append($"&status={Uri.EscapeDataString(status)}");

            var res = await client.GetAsync(url.ToString());

            if (res.IsSuccessStatusCode)
            {
                var json = await res.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<UsersListResponse>(json, JsonOpts);

                Users.Clear();

                if (data?.Data == null)
                {
                    TotalPages = 1;
                    TotalUsers = 0;
                    return;
                }

                foreach (var u in data.Data)
                {
                    u.FullName ??= string.Empty;
                    u.Username ??= string.Empty;
                    u.Email ??= string.Empty;
                    u.Role ??= string.Empty;
                    u.Status ??= string.Empty;
                    u.CreatedAtDisplay = FormatIsoDate(u.CreatedAt);
                    Users.Add(u);
                }

                TotalPages = data.Pagination?.TotalPages ?? 1;
                TotalUsers = data.Pagination?.Total ?? data.Data.Count;
                return;
            }

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
            IsLoading = false;
        }
    }

    private async Task ShowUserDetailAsync(int userId)
    {
        try
        {
            IsLoading = true;

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
                    Content = new TextBlock { Text = content.ToString(), TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap },
                    CloseButtonText = "Close",
                    XamlRoot = App.MainWindow.Content.XamlRoot
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
            IsLoading = false;
        }
    }

    private async Task ToggleUserStatusAsync(int userId)
    {
        try
        {
            IsLoading = true;

            var client = CreateClient();

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
            IsLoading = false;
        }
    }

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

    public sealed class UserItem
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
        public string CreatedAtDisplay { get; set; } = string.Empty;
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
}
