using CSharpFunctionalExtensions;
using System.Threading.Tasks;
using Vidora.Core.Contracts.Results;

namespace Vidora.Core.Interfaces.Api;

public interface IWatchlistApiService
{
    Task<Result<WatchlistResult>> GetWatchlistAsync(int page = 1, int limit = 10);
    Task<Result<ToggleWatchlistResult>> AddToWatchlistAsync(int movieId);
    Task<Result<ToggleWatchlistResult>> RemoveFromWatchlistAsync(int movieId);
}
