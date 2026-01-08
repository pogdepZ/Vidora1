using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Vidora.Core.Contracts.Services;

namespace Vidora.Presentation.Gui.Views
{
    public sealed partial class AdminDashboardPage : Page
    {
        private readonly ISessionStateService _sessionState = App.GetService<ISessionStateService>();
        private static readonly HttpClient _http = new HttpClient();

        public AdminDashboardPage()
        {
            InitializeComponent();
            Loaded += AdminDashboardPage_Loaded;
        }

        private async void AdminDashboardPage_Loaded(object sender, RoutedEventArgs e)
        {
            CurrentDateText.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

            try
            {
                var token = _sessionState.AccessToken?.Token;

                if (string.IsNullOrWhiteSpace(token))
                {
                    await ShowDialog("Auth", "Không lấy được token từ session. Bạn hãy đăng nhập lại.");
                    return;
                }

                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var url = "http://localhost:3000/api/stats/dashboard";

             
                var json = await _http.GetStringAsync(url);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var dto = JsonSerializer.Deserialize<DashboardDto>(json, options);

                if (dto == null || dto.Success == false)
                {
                    await ShowDialog("API Error", "Không lấy được dữ liệu dashboard.");
                    return;
                }

                // TEXT CARDS
                TotalMoviesText.Text = dto.TotalMovies.ToString();
                TotalUsersText.Text = dto.TotalUsers.ToString();
                TotalViewsText.Text = dto.TodayViews.ToString();
                NewSignupsText.Text = dto.TotalTodayNewUsers.ToString();

                // API bạn gửi chưa có MonthlyRevenue => set tạm 0
                RevenueText.Text = "0";

                // LISTS
                RecentSignupsItems.ItemsSource = dto.NewUsers ?? new List<UserDto>();
                MostWatchedMoviesItems.ItemsSource = dto.MostWatchedMovies ?? new List<MovieDto>();
            }
            catch (HttpRequestException ex)
            {
                await ShowDialog("HTTP Error", ex.Message);
            }
            catch (Exception ex)
            {
                await ShowDialog("Error", ex.Message);
            }
        }

        private async System.Threading.Tasks.Task ShowDialog(string title, string content)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = content,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }

        // ===== DTOs khớp với JSON bạn đưa =====
        public sealed class DashboardDto
        {
            public bool Success { get; set; }
            public int TotalUsers { get; set; }
            public int TotalTodayNewUsers { get; set; }
            public List<UserDto>? NewUsers { get; set; }
            public int TodayViews { get; set; }
            public int TotalMovies { get; set; }

            // JSON: "MostWatchedMovies"
            public List<MovieDto>? MostWatchedMovies { get; set; }

            public List<MovieDto>? HighestRatedMovies { get; set; }

            // JSON: "revenueByDayinCurrentMonth"
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
}
