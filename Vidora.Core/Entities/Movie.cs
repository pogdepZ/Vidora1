using System.Collections.Generic;

namespace Vidora.Core.Entities;

public class Movie
{
    public required int MovieId { get; init; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public int? ReleaseYear { get; set; }
    public string? PosterUrl { get; set; }
    public string? BannerUrl { get; set; }
    public string? TrailerUrl { get; set; }
    public string? MovieUrl { get; set; }
    public double AvgRating { get; set; }
    public IReadOnlyList<string> Genres { get; set; } = [];

    // Thay đổi từ string sang MovieMember
    public IReadOnlyList<MovieMember> Actors { get; set; } = [];
}