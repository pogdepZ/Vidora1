using CSharpFunctionalExtensions;
using System;
using System.Threading.Tasks;
using Vidora.Core.Contracts.Results;
using Vidora.Core.Interfaces.Api;

namespace Vidora.Core.UseCases;

public class RemoveFromWatchlistUseCase
{
    private readonly IWatchlistApiService _watchlistApiService;

    public RemoveFromWatchlistUseCase(IWatchlistApiService watchlistApiService)
    {
        _watchlistApiService = watchlistApiService;
    }

    public async Task<Result<ToggleWatchlistResult>> ExecuteAsync(int movieId)
    {
        try
        {
            if (movieId <= 0)
            {
                return Result.Failure<ToggleWatchlistResult>("ID phim không hợp lệ");
            }

            return await _watchlistApiService.RemoveFromWatchlistAsync(movieId);
        }
        catch (Exception ex)
        {
            return Result.Failure<ToggleWatchlistResult>($"Lỗi xóa khỏi danh sách yêu thích: {ex.Message}");
        }
    }
}
