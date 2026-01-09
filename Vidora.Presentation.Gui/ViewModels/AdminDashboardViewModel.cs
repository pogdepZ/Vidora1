using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Vidora.Core.Contracts.Services;
using Vidora.Presentation.Gui.Contracts.ViewModels;
using Windows.Devices.Geolocation;

namespace Vidora.Presentation.Gui.ViewModels;

public partial class AdminDashboardViewModel : ObservableRecipient, INavigationAware
{
    private static readonly HttpClient HttpClient = new();
    private readonly ISessionStateService _sessionStateService;

    [ObservableProperty]
    private string _currentDateText = "-";

    [ObservableProperty]
    private string _totalMoviesText = "-";

    [ObservableProperty]
    private string _totalUsersText = "-";

    [ObservableProperty]
    private string _totalViewsText = "-";

    [ObservableProperty]
    private string _revenueText = "0";

    [ObservableProperty]
    private string _newSignupsText = "-";

    [ObservableProperty]
    private ObservableCollection<UserDto> _recentSignups = [];

    [ObservableProperty]
    private ObservableCollection<MovieDto> _highestRatedMovies = [];

    [ObservableProperty] private ObservableCollection<double> _revenueData = [];
    [ObservableProperty] private ObservableCollection<string> _revenueLabels = [];
    public AdminDashboardViewModel(ISessionStateService sessionStateService)
    {
        _sessionStateService = sessionStateService;
    }

    public async Task OnNavigatedToAsync(object parameter)
    {
        CurrentDateText = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

        try
        {
            var token = _sessionStateService.AccessToken?.Token;

            if (string.IsNullOrWhiteSpace(token))
            {
                await ShowDialogAsync("Auth", "Không lấy được token từ session. Bạn hãy đăng nhập lại.");
                return;
            }

            HttpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var url = "http://localhost:3000/api/stats/dashboard";
            var json = await HttpClient.GetStringAsync(url);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var dto = JsonSerializer.Deserialize<DashboardDto>(json, options);

            if (dto == null || dto.Success == false)
            {
                await ShowDialogAsync("API Error", "Không lấy được dữ liệu dashboard.");
                return;
            }

            TotalMoviesText = dto.TotalMovies.ToString();
            TotalUsersText = dto.TotalUsers.ToString();
            TotalViewsText = dto.TodayViews.ToString();
            NewSignupsText = dto.TotalTodayNewUsers.ToString();

            var labels = dto.RevenueByDayinCurrentMonth?.Labels ?? new List<string>();
            var data = dto.RevenueByDayinCurrentMonth?.Data ?? new List<double>();

            RevenueLabels = new ObservableCollection<string>(labels);
            RevenueData = new ObservableCollection<double>(data);

            // ✅ RevenueText: tổng doanh thu tháng (sum)
            var totalRevenue = data.Sum();
            RevenueText = totalRevenue.ToString("N0");

            RecentSignups = new ObservableCollection<UserDto>(dto.NewUsers ?? new List<UserDto>());
            HighestRatedMovies = new ObservableCollection<MovieDto>(dto.HighestRatedMovies ?? new List<MovieDto>());
        }
        catch (HttpRequestException ex)
        {
            await ShowDialogAsync("HTTP Error", ex.Message);
        }
        catch (Exception ex)
        {
            await ShowDialogAsync("Error", ex.Message);
        }
    }

    public async Task OnNavigatedFromAsync()
    {
    }

    private static async Task ShowDialogAsync(string title, string content)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = content,
            CloseButtonText = "OK",
            XamlRoot = App.MainWindow.Content.XamlRoot
        };
        await dialog.ShowAsync();
    }

    public sealed class DashboardDto
    {
        public bool Success { get; set; }
        public int TotalUsers { get; set; }
        public int TotalTodayNewUsers { get; set; }
        public List<UserDto>? NewUsers { get; set; }
        public int TodayViews { get; set; }
        public int TotalMovies { get; set; }
        public List<MovieDto>? MostWatchedMovies { get; set; }
        public List<MovieDto>? HighestRatedMovies { get; set; }
        public RevenueByDayDto? RevenueByDayinCurrentMonth { get; set; }
    }

    public sealed class UserDto
    {
        public int UserId { get; set; }
        public string? FullName { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Avatar { get; set; }
        public string? Role { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? Gender { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? Birthday { get; set; }
    }

    public sealed class MovieDto
    {
        public int MovieId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int ReleaseYear { get; set; }
        public string? PosterUrl { get; set; }
        public string? TrailerUrl { get; set; }
        public string? MovieUrl { get; set; }
        public string? BannerUrl { get; set; }
        public bool IsDeleted { get; set; }
        public double AvgRating { get; set; }
        public int RatingCount { get; set; }
    }

    public sealed class RevenueByDayDto
    {
        public List<string>? Labels { get; set; }
        public List<double>? Data { get; set; }
    }
}
