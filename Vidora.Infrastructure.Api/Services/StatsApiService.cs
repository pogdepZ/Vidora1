using AutoMapper;
using CSharpFunctionalExtensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vidora.Core.Contracts.Results;
using Vidora.Core.Contracts.Services;
using Vidora.Core.Entities;
using Vidora.Core.Interfaces.Api;
using Vidora.Infrastructure.Api.Dtos.Responses.Datas;
using Vidora.Core.Helpers;

namespace Vidora.Infrastructure.Api.Services;

public class StatsApiService : IStatsApiService
{
    private readonly ApiClient _apiClient;
    private readonly IMapper _mapper;
    private readonly ISessionStateService _sessionService;

    public StatsApiService(ApiClient apiClient, IMapper mapper, ISessionStateService sessionService)
    {
        _apiClient = apiClient;
        _mapper = mapper;
        _sessionService = sessionService;
    }

    public async Task<Result<DashboardResult>> GetDashboardAsync()
    {
        var tokenObject = _sessionService.AccessToken;
        var accessToken = tokenObject?.Token;

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return Result.Failure<DashboardResult>("Access token not found. Please login again.");
        }

        var url = "api/stats/dashboard";

        var httpRes = await _apiClient.GetAsync(url, token: accessToken);
        var json = await httpRes.Content.ReadAsStringAsync();

        if (!httpRes.IsSuccessStatusCode)
        {
            return Result.Failure<DashboardResult>("Không th? t?i d? li?u dashboard");
        }

        // The dashboard response has fields directly on root (not wrapped in "data")
        if (!JsonHelper.TryDeserialize<DashboardData>(json, out var dashboardData) || dashboardData == null)
        {
            return Result.Failure<DashboardResult>("Không th? ð?c d? li?u dashboard");
        }

        var result = new DashboardResult
        {
            TotalUsers = dashboardData.TotalUsers,
            TotalTodayNewUsers = dashboardData.TotalTodayNewUsers,
            NewUsers = _mapper.Map<IReadOnlyList<User>>(dashboardData.NewUsers ?? new List<UserData>()),
            TodayViews = dashboardData.TodayViews,
            TotalMovies = dashboardData.TotalMovies,
            MostWatchedMovies = _mapper.Map<IReadOnlyList<Movie>>(dashboardData.MostWatchedMovies ?? new List<MovieData>()),
            HighestRatedMovies = _mapper.Map<IReadOnlyList<Movie>>(dashboardData.HighestRatedMovies ?? new List<MovieData>())
        };

        return Result.Success(result);
    }
}
