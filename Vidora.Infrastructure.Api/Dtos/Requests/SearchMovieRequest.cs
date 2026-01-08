namespace Vidora.Infrastructure.Api.Dtos.Requests;

internal record SearchMovieRequest(
    string? Title,
    string? Genre,
    int? ReleaseYear,
    int Page,
    int Limit
);
