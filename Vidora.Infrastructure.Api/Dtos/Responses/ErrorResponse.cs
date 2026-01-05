using System.Net;

namespace Vidora.Infrastructure.Api.Dtos.Responses;

internal record ErrorResponse(
    object Error,
    HttpStatusCode StatusCode,
    string? Message = null
) : ApiResponse(StatusCode, Message)
{
    public override bool Success => false;
}