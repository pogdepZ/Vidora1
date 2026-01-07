using System.Collections.Generic;
using Vidora.Core.Entities;

namespace Vidora.Core.Contracts.Results;

public class DashboardResult
{
    public int TotalUsers { get; set; }
    public int TotalTodayNewUsers { get; set; }
    public IReadOnlyList<User> NewUsers { get; set; } = [];
    public int TodayViews { get; set; }
    public int TotalMovies { get; set; }
    public IReadOnlyList<Movie> MostWatchedMovies { get; set; } = [];
    public IReadOnlyList<Movie> HighestRatedMovies { get; set; } = [];
}