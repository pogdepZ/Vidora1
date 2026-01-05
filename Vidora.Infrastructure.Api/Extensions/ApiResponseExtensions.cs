using System.Net.Http;
using System.Threading.Tasks;
using Vidora.Core.Helpers;
using Vidora.Infrastructure.Api.Dtos.Responses;

namespace Vidora.Infrastructure.Api.Extensions;

internal static class ApiResponseExtensions
{
    public static async Task<ApiResponse> ReadAsync<T>(
        this HttpResponseMessage response)
    {
        var statusCode = response.StatusCode;
        var json = await response.Content.ReadAsStringAsync();

        // HTTP error
        if (!response.IsSuccessStatusCode)
        {
            if (JsonHelper.TryDeserialize<ErrorResponse>(json, out var error)
                && error != null)
            {
                return error with { StatusCode = statusCode };
            }

            return new ErrorResponse(
                Error: json,
                StatusCode: statusCode,
                Message: "Http request failed"
            );
        }

        // HTTP success
        if (JsonHelper.TryDeserialize<SuccessResponse<T>>(json, out var success)
            && success != null)
        {
            return success with { StatusCode = statusCode };
        }

        return new ErrorResponse(
            Error: json,
            StatusCode: statusCode,
            Message: "Invalid response format"
        );
    }

    public static async Task<ApiResponse> ReadListAsync<T>(
        this HttpResponseMessage response)
    {
        var statusCode = response.StatusCode;
        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            return new ErrorResponse(
                Error: json,
                StatusCode: statusCode,
                Message: "Http request failed"
            );
        }

        if (JsonHelper.TryDeserialize<ListSuccessResponse<T>>(json, out var success)
            && success != null)
        {
            return success with { StatusCode = statusCode };
        }

        return new ErrorResponse(
            Error: json,
            StatusCode: statusCode,
            Message: "Invalid list response format"
        );
    }

    public static async Task<ApiResponse> ReadPaginatedAsync<T>(
        this HttpResponseMessage response)
    {
        var statusCode = response.StatusCode;
        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            return new ErrorResponse(
                Error: json,
                StatusCode: statusCode,
                Message: "Http request failed"
            );
        }

        if (JsonHelper.TryDeserialize<PaginatedSucessResponse<T>>(json, out var success)
            && success != null)
        {
            return success with { StatusCode = statusCode };
        }

        return new ErrorResponse(
            Error: json,
            StatusCode: statusCode,
            Message: "Invalid paginated response format"
        );
    }
}
