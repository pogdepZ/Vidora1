using System.Collections.Generic;

namespace Vidora.Core.Contracts.Commands;

public class CreateMovieCommand
{
    public string Title { get; }
    public string Description { get; }
    public string PosterUrl { get; }
    public string BannerUrl { get; }
    public string TrailerUrl { get; }
    public string MovieUrl { get; }
    public int ReleaseYear { get; }

    // Thay đổi: Dùng List<int> để chứa ID thay vì tên
    public List<int> GenreIds { get; }

    // Thay đổi: Dùng List<(int MemberId, string Role)> để chứa ID và Role
    public List<(int MemberId, string Role)> CastAndCrew { get; }

    public CreateMovieCommand(
        string title,
        string description,
        string posterUrl,
        string bannerUrl,
        string trailerUrl,
        string movieUrl,
        int releaseYear,
        List<int> genreIds,
        List<(int MemberId, string Role)> castAndCrew)
    {
        Title = title;
        Description = description;
        PosterUrl = posterUrl;
        BannerUrl = bannerUrl;
        TrailerUrl = trailerUrl;
        MovieUrl = movieUrl;
        ReleaseYear = releaseYear;
        GenreIds = genreIds;
        CastAndCrew = castAndCrew;
    }
}