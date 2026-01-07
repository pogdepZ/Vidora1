using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Vidora.Core.Helpers;

namespace Vidora.Infrastructure.Api.Clients;

public class ApiClient
{
    private readonly HttpClient _httpClient;

    public ApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task<HttpResponseMessage> GetAsync(
        string path,
        Action<HttpRequestHeaders>? headers = null,
        CancellationToken ct = default)
        => SendAsync(HttpMethod.Get, path, null, headers, ct);

    public Task<HttpResponseMessage> PostAsync(
        string path,
        object? body,
        Action<HttpRequestHeaders>? headers = null,
        CancellationToken ct = default)
        => SendAsync(HttpMethod.Post, path, body, headers, ct);

    public Task<HttpResponseMessage> PutAsync(
        string url,
        object? body,
        Action<HttpRequestHeaders>? headers = null,
        CancellationToken ct = default)
        => SendAsync(HttpMethod.Put, url, body, headers, ct);

    public Task<HttpResponseMessage> PatchAsync(
        string url,
        object? body,
        Action<HttpRequestHeaders>? headers = null,
        CancellationToken ct = default)
    => SendAsync(HttpMethod.Patch, url, body, headers, ct);

    public Task<HttpResponseMessage> DeleteAsync(
        string url,
        Action<HttpRequestHeaders>? headers = null,
        CancellationToken ct = default)
        => SendAsync(HttpMethod.Delete, url, null, headers, ct);

    private async Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string url,
        object? body,
        Action<HttpRequestHeaders>? headers,
        CancellationToken ct)
    {
        using var request = new HttpRequestMessage(method, url);

        // JSON body
        if (body != null)
            request.Content = JsonContent.Create(body, options: JsonHelper.CamelCaseOptions);

        // Extra headers
        headers?.Invoke(request.Headers);

        // Send request
        var response = await _httpClient.SendAsync(request, ct);

        return response;
    }
}