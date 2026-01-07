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
using Vidora.Infrastructure.Api.Clients;

namespace Vidora.Infrastructure.Api.Services;

public class StatsApiService : IStatsApiService
{
    private readonly ApiClient _apiClient;
    private readonly IMapper _mapper;

    public StatsApiService(ApiClient apiClient, IMapper mapper)
    {
        _apiClient = apiClient;
        _mapper = mapper;
    }

    public async Task<Result<DashboardResult>> GetDashboardAsync()
    {

        var url = "api/stats/dashboard";

        var httpRes = await _apiClient.GetAsync(url);
        var json = await httpRes.Content.ReadAsStringAsync();

        if (!httpRes.IsSuccessStatusCode)
        {
            return Result.Failure<DashboardResult>("Không thể tải dữ liệu dashboard");
        }

        if (!JsonHelper.TryDeserialize<DashboardData>(json, out var dashboardData) || dashboardData == null)
        {
            return Result.Failure<DashboardResult>("Không thể đọc dữ liệu dashboard");
        }

        var result = new DashboardResult
        {
            TotalUsers = dashboardData.TotalUsers,
            TotalTodayNewUsers = dashboardData.TotalTodayNewUsers,
            NewUsers = _mapper.Map<IReadOnlyList<User>>(dashboardData.NewUsers ?? []),
            TodayViews = dashboardData.TodayViews,
            TotalMovies = dashboardData.TotalMovies,
            MostWatchedMovies = _mapper.Map<IReadOnlyList<Movie>>(dashboardData.MostWatchedMovies ?? []),
            HighestRatedMovies = _mapper.Map<IReadOnlyList<Movie>>(dashboardData.HighestRatedMovies ?? [])
        };

        return Result.Success(result);
    }
}
