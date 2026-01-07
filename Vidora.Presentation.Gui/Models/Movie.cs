using System.Collections.Generic;
using System.Linq;

namespace Vidora.Presentation.Gui.Models;

public class Movie
{
    public int MovieId { get; set; }
    public string Id { get; set; } = string.Empty;   // UI-only
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
    public int? ReleaseYear { get; set; }
    public string? PosterUrl { get; set; } = string.Empty;
    public string? BannerUrl { get; set; } = string.Empty;
    public string? VideoUrl { get; set; } = string.Empty;
    public string? TrailerUrl { get; set; } = string.Empty;
    public string? MovieUrl { get; set; } = string.Empty;
    public double AvgRating { get; set; }
    public double Rating { get; set; }               // UI-only
    public int? UserRating { get; set; }             // User's rating for this movie
    public bool IsInWatchlist { get; set; }          // Whether movie is in user's watchlist
    public int DurationMinutes { get; set; }         // UI-only
    public IReadOnlyList<string> Genres { get; set; } = [];
    public IReadOnlyList<MovieMember> Actors { get; set; } = [];

    /// <summary>
    /// Hiển thị danh sách thể loại dạng chuỗi, cách nhau bởi dấu " • "
    /// </summary>
    public string GenresDisplay => Genres.Count > 0 ? string.Join(" • ", Genres.Take(3)) : "Chưa phân loại";
}
