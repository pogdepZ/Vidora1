using System.Collections.Generic;
using Vidora.Core.Entities;

namespace Vidora.Core.Contracts.Results;

public class GenreListResult
{
    public IReadOnlyList<Genre> Genres { get; set; } = [];
}
