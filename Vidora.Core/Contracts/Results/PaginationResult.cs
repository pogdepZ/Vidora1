using System;

namespace Vidora.Core.Contracts.Results;

public record PaginationResult(
    int Page,
    int Limit,
    int Total,
    int TotalPages
)
{
    public bool HasPrev => Page > 1;
    public bool HasNext => Page < TotalPages;
    public int Count => CountItems();

    private int CountItems()
    {
        if (Total == 0) return 0;
        return Math.Min(Limit, Math.Max(0, Total - (Page - 1) * Limit));
    }
}
