using CSharpFunctionalExtensions;
using System;
using System.Threading.Tasks;
using Vidora.Core.Contracts.Results;
using Vidora.Core.Interfaces.Api;

namespace Vidora.Core.UseCases;

public class GetGenresUseCase
{
    private readonly IMovieApiService _movieApiService;

    public GetGenresUseCase(IMovieApiService movieApiService)
    {
        _movieApiService = movieApiService;
    }

    public async Task<Result<GenreListResult>> ExecuteAsync()
    {
        try
        {
            return await _movieApiService.GetGenresAsync();
        }
        catch (Exception ex)
        {
            return Result.Failure<GenreListResult>($"Failed to get genres: {ex.Message}");
        }
    }
}
