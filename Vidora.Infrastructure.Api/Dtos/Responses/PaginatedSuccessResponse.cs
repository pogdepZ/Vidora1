using System.Collections.Generic;
using System.Net;
using Vidora.Infrastructure.Api.Dtos.Responses.Metas;

namespace Vidora.Infrastructure.Api.Dtos.Responses;

internal record PaginatedSuccessResponse<T>(
    IReadOnlyList<T> Data,
    Pagination Pagination,
    HttpStatusCode StatusCode,
    string? Message = null
) : SuccessResponse<IReadOnlyList<T>>(Data, StatusCode, Message);
