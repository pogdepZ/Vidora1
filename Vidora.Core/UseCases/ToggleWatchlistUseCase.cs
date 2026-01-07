using CSharpFunctionalExtensions;
using System;
using System.Threading.Tasks;
using Vidora.Core.Contracts.Results;
using Vidora.Core.Interfaces.Api;

namespace Vidora.Core.UseCases;

public class ToggleWatchlistUseCase
{
    private readonly IWatchlistApiService _watchlistApiService;

    public ToggleWatchlistUseCase(IWatchlistApiService watchlistApiService)
    {
        _watchlistApiService = watchlistApiService;
    }

    public async Task<Result<ToggleWatchlistResult>> ExecuteAsync(int movieId)
    {
        try
        {
            if (movieId <= 0)
            {
                return Result.Failure<ToggleWatchlistResult>("ID phim không h?p l?");
            }

            return await _watchlistApiService.ToggleWatchlistAsync(movieId);
        }
        catch (Exception ex)
        {
            return Result.Failure<ToggleWatchlistResult>($"L?i c?p nh?t danh sách yêu thích: {ex.Message}");
        }
    }
}
