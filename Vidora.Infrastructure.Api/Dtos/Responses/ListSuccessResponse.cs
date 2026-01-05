using System.Collections.Generic;
using System.Net;

namespace Vidora.Infrastructure.Api.Dtos.Responses;

internal record ListSuccessResponse<T>(
    IReadOnlyList<T> Data,
    HttpStatusCode StatusCode,
    string? Message = null
) : SuccessResponse<IReadOnlyList<T>>(Data, StatusCode, Message);