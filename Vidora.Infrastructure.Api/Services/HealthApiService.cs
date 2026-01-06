using CSharpFunctionalExtensions;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Vidora.Core.Interfaces.Api;
using Vidora.Infrastructure.Api.Clients;
using Vidora.Infrastructure.Api.Extensions;

namespace Vidora.Infrastructure.Api.Services;

public class HealthApiService : IHealthApiService
{
    private readonly ApiClient _apiClient;
    public HealthApiService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<Result> CheckHealthAsync()
    {
        try
        {
            var httpRes = await _apiClient.GetAsync("health");
            var apiRes = await httpRes.ReadAsync();

            if (!apiRes.Success)
                return Result.Failure(apiRes.Message ?? "Server is unhealthy");

            return Result.Success(apiRes.Message);
        }
        catch (Exception ex) when (
            ex is HttpRequestException ||
            ex is TaskCanceledException)
        {
            return Result.Failure("Unable to connect to server");
        }
    }

}
