using System.Collections.Generic;
using Vidora.Core.Entities;

namespace Vidora.Core.Contracts.Results;

public class SearchMovieResult
{
    public IReadOnlyList<Movie> Movies { get; set; } = [];
    public PaginationResult Pagination { get; set; } = new();
}
