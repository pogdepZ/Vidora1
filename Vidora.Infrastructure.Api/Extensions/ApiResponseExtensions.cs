using System.Net;
using Vidora.Core.Exceptions;
using Vidora.Infrastructure.Api.Dtos.Responses;

namespace Vidora.Infrastructure.Api.Extensions;

internal static class ApiResponseExtensions
{
    public static SuccessResponse<T> EnsureSuccess<T>(this ApiResponse response)
    {
        if (response is ErrorResponse error)
            throw MapDomainException(error);

        if (response is not SuccessResponse<T> success)
            throw new DomainException("Invalid response type");

        return success;
    }

    public static DomainException MapDomainException(ErrorResponse error)
    {
        return error.StatusCode switch
        {
            HttpStatusCode.BadRequest =>
                new DomainException(error.Message ?? "Bad request error"),

            HttpStatusCode.Unauthorized =>
                new UnauthorizationException(error.Message ?? "Unauthorization error"),

            HttpStatusCode.Forbidden =>
                new ForbiddenException(error.Message ?? "Not permission"),

            HttpStatusCode.Conflict =>
                new ConflictException(error.Message ?? "Conflict occurred"),

            HttpStatusCode.NotFound =>
                new NotFoundException(error.Message ?? "Resource not found"),

            HttpStatusCode.UnprocessableEntity =>
                new ValidationException(error.Message ?? "Validation error"),

            _ =>
                new DomainException(error.Message ?? "Unexpected error")
        };
    }
}
