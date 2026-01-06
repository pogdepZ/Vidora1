using System.Collections.Generic;

namespace Vidora.Infrastructure.Api.Dtos.Responses.Datas;

internal record MovieData(
    int MovieId,
    string Title,
    string? Description,
    int? ReleaseYear,
    string? PosterUrl,
    string? BannerUrl,
    string? TrailerUrl,
    string? MovieUrl,
    double AvgRating,
    IReadOnlyList<string> Genres,
    IReadOnlyList<ActorData> Actors
);
