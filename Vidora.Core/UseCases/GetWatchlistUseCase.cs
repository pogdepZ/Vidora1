using CSharpFunctionalExtensions;
using System;
using System.Threading.Tasks;
using Vidora.Core.Contracts.Results;
using Vidora.Core.Interfaces.Api;

namespace Vidora.Core.UseCases;

public class GetWatchlistUseCase
{
    private readonly IWatchlistApiService _watchlistApiService;

    public GetWatchlistUseCase(IWatchlistApiService watchlistApiService)
    {
        _watchlistApiService = watchlistApiService;
    }

    public async Task<Result<WatchlistResult>> ExecuteAsync(int page = 1, int limit = 10)
    {
        try
        {
            return await _watchlistApiService.GetWatchlistAsync(page, limit);
        }
        catch (Exception ex)
        {
            return Result.Failure<WatchlistResult>($"L?i t?i danh sách yêu thích: {ex.Message}");
        }
    }
}
