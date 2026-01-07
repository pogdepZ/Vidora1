using AutoMapper;
using CSharpFunctionalExtensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vidora.Core.Contracts.Results;
using Vidora.Core.Contracts.Services;
using Vidora.Core.Entities;
using Vidora.Core.Interfaces.Api;
using Vidora.Infrastructure.Api.Dtos.Responses;
using Vidora.Infrastructure.Api.Dtos.Responses.Datas;
using Vidora.Infrastructure.Api.Extensions;

namespace Vidora.Infrastructure.Api.Services;

public class WatchlistApiService : IWatchlistApiService
{
    private readonly ApiClient _apiClient;
    private readonly IMapper _mapper;
    private readonly ISessionStateService _sessionService;

    public WatchlistApiService(ApiClient apiClient, IMapper mapper, ISessionStateService sessionService)
    {
        _apiClient = apiClient;
        _mapper = mapper;
        _sessionService = sessionService;
    }

    public async Task<Result<WatchlistResult>> GetWatchlistAsync(int page = 1, int limit = 10)
    {
        var tokenObject = _sessionService.AccessToken;
        var accessToken = tokenObject?.Token;

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return Result.Failure<WatchlistResult>("Access token not found. Please login again.");
        }

        var url = $"api/watchlist?page={page}&limit={limit}";

        var httpRes = await _apiClient.GetAsync(url, token: accessToken);

        var apiRes = await httpRes.ReadPaginatedAsync<MovieData>();

        if (apiRes is not PaginatedSucessResponse<MovieData> success)
        {
            return Result.Failure<WatchlistResult>(
                apiRes.Message ?? "Lấy danh sách yêu thích thất bại"
            );
        }

        var result = new WatchlistResult
        {
            Movies = _mapper.Map<IReadOnlyList<Movie>>(success.Data),
            Pagination = _mapper.Map<PaginationResult>(success.Pagination)
        };

        return Result.Success(result);
    }

    public async Task<Result<ToggleWatchlistResult>> AddToWatchlistAsync(int movieId)
    {
        var tokenObject = _sessionService.AccessToken;
        var accessToken = tokenObject?.Token;

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return Result.Failure<ToggleWatchlistResult>("Access token not found. Please login again.");
        }

        var url = $"api/watchlist/{movieId}";

        var httpRes = await _apiClient.PostAsync(url, null, token: accessToken);

        if (httpRes.IsSuccessStatusCode)
        {
            return Result.Success(new ToggleWatchlistResult
            {
                IsInWatchlist = true,
                Message = "Đã thêm vào danh sách yêu thích"
            });
        }

        var apiRes = await httpRes.ReadAsync<object>();
        return Result.Failure<ToggleWatchlistResult>(
            apiRes.Message ?? "Thêm vào danh sách yêu thích thất bại"
        );
    }

    public async Task<Result<ToggleWatchlistResult>> RemoveFromWatchlistAsync(int movieId)
    {
        var tokenObject = _sessionService.AccessToken;
        var accessToken = tokenObject?.Token;

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return Result.Failure<ToggleWatchlistResult>("Access token not found. Please login again.");
        }

        var url = $"api/watchlist/{movieId}";

        var httpRes = await _apiClient.DeleteAsync(url, token: accessToken);

        if (httpRes.IsSuccessStatusCode)
        {
            return Result.Success(new ToggleWatchlistResult
            {
                IsInWatchlist = false,
                Message = "Đã xóa khỏi danh sách yêu thích"
            });
        }

        var apiRes = await httpRes.ReadAsync<object>();
        return Result.Failure<ToggleWatchlistResult>(
            apiRes.Message ?? "Xóa khỏi danh sách yêu thích thất bại"
        );
    }
}
