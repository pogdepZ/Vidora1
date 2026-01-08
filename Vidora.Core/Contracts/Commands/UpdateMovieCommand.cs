using System.Collections.Generic;

namespace Vidora.Core.Contracts.Commands;

public class UpdateMovieCommand
{
    public int MovieId { get; }
    public string Title { get; }
    public string Description { get; }
    public string PosterUrl { get; }
    public string BannerUrl { get; }
    public string TrailerUrl { get; }
    public string MovieUrl { get; }
    public int ReleaseYear { get; }
    public List<int> GenreIds { get; }
    public List<(int MemberId, string Role)> CastAndCrew { get; }

    public UpdateMovieCommand(
        int movieId,
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
        MovieId = movieId;
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