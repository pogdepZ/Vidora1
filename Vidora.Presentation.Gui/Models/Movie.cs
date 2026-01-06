using System.Collections.Generic;

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
    public int DurationMinutes { get; set; }         // UI-only
    public IReadOnlyList<string> Genres { get; set; } = [];
    public IReadOnlyList<MovieMember> Actors { get; set; } = [];
}
