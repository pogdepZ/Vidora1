namespace Vidora.Core.Contracts.Commands;

public class SearchMovieCommand
{
    public string? Title { get; set; }
    public int? GenreId { get; set; }
    public int? ReleaseYear { get; set; }
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 8;
}
