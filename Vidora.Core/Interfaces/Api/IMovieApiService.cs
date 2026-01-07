using CSharpFunctionalExtensions;
using System.Threading.Tasks;
using Vidora.Core.Contracts.Commands;
using Vidora.Core.Contracts.Results;

namespace Vidora.Core.Interfaces.Api;

public interface IMovieApiService
{
    Task<Result<SearchMovieResult>> SearchMoviesAsync(SearchMovieCommand command);
    Task<Result<MovieDetailResult>> GetMovieDetailAsync(int movieId);
    Task<Result<GenreListResult>> GetGenresAsync();
    Task<Result<RateMovieResult>> RateMovieAsync(int movieId, int rating);
}
