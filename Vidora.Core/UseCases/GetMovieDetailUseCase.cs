using CSharpFunctionalExtensions;
using System;
using System.Threading.Tasks;
using Vidora.Core.Contracts.Results;
using Vidora.Core.Interfaces.Api;

namespace Vidora.Core.UseCases;

public class GetMovieDetailUseCase
{
    private readonly IMovieApiService _movieApiService;

    public GetMovieDetailUseCase(IMovieApiService movieApiService)
    {
        _movieApiService = movieApiService;
    }

    public async Task<Result<MovieDetailResult>> ExecuteAsync(int movieId)
    {
        try
        {
            if (movieId <= 0)
            {
                return Result.Failure<MovieDetailResult>("Invalid movie ID");
            }

            return await _movieApiService.GetMovieDetailAsync(movieId);
        }
        catch (Exception ex)
        {
            return Result.Failure<MovieDetailResult>($"Failed to get movie detail: {ex.Message}");
        }
    }
}
