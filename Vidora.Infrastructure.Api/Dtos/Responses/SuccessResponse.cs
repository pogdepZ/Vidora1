using System.Net;

namespace Vidora.Infrastructure.Api.Dtos.Responses;

internal record SuccessResponse(
    HttpStatusCode StatusCode,
    string? Message = null
) : ApiResponse(StatusCode, Message)
{
    public override bool Success => true;
}

internal record SuccessResponse<T>(
    T Data,
    HttpStatusCode StatusCode,
    string? Message = null
) : SuccessResponse(StatusCode, Message);
