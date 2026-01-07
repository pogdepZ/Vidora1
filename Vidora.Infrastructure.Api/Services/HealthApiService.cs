using CSharpFunctionalExtensions;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Vidora.Core.Interfaces.Api;
using Vidora.Infrastructure.Api.Clients;

namespace Vidora.Infrastructure.Api.Services;

public class HealthApiService : IHealthApiService
{
    private readonly ApiClient _apiClient;
    public HealthApiService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task CheckHealthAsync()
    {
        return  _apiClient.GetAsync(
            path: "health"
            );
    }
}
