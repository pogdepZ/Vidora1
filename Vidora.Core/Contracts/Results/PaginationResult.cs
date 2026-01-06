using System;

namespace Vidora.Core.Contracts.Results;

public record PaginationResult
{
    public int Page { get; init; }
    public int Limit { get; init; }
    public int Total { get; init; }
    public int TotalPages { get; init; }

    public bool HasPrev => Page > 1;
    public bool HasNext => Page < TotalPages;

    public PaginationResult() { }

    public PaginationResult(int page, int limit, int total, int totalPages)
    {
        Page = page;
        Limit = limit;
        Total = total;
        TotalPages = totalPages;
    }
}
