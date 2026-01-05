using System.Net;

namespace Vidora.Infrastructure.Api.Dtos.Responses;

internal record SuccessResponse<T>(
    T Data,
    HttpStatusCode StatusCode,
    string? Message = null
) : ApiResponse(StatusCode, Message)
{
    public override bool Success => true;
}