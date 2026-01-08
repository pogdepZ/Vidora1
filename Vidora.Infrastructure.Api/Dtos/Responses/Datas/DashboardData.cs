using System.Collections.Generic;

namespace Vidora.Infrastructure.Api.Dtos.Responses.Datas;

internal record DashboardData(
    int TotalUsers,
    int TotalTodayNewUsers,
    IReadOnlyList<UserData>? NewUsers,
    int TodayViews,
    int TotalMovies,
    IReadOnlyList<MovieData>? MostWatchedMovies,
    IReadOnlyList<MovieData>? HighestRatedMovies
);