using System.Net;

namespace Vidora.Infrastructure.Api.Dtos.Responses;

internal record ErrorResponse(
    HttpStatusCode StatusCode,
    string? Message = null,
    object? Error = null
) : ApiResponse(StatusCode, Message)
{
    public override bool Success => false;
}
