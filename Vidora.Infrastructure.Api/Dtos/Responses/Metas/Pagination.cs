namespace Vidora.Infrastructure.Api.Dtos.Responses.Metas;

internal record Pagination(
    int Page,
    int Limit,
    int Total,
    int TotalPages
);
