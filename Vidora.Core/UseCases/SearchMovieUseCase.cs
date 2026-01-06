using CSharpFunctionalExtensions;
using System;
using System.Threading.Tasks;
using Vidora.Core.Contracts.Commands;
using Vidora.Core.Contracts.Results;
using Vidora.Core.Interfaces.Api;

namespace Vidora.Core.UseCases;

public class SearchMovieUseCase
{
    private readonly IMovieApiService _movieApiService;

    public SearchMovieUseCase(IMovieApiService movieApiService)
    {
        _movieApiService = movieApiService;
    }

    public async Task<Result<SearchMovieResult>> ExecuteAsync(SearchMovieCommand command)
    {
        try
        {
            // Validate command
            if (command.Page < 1) command.Page = 1;
            if (command.Limit < 1) command.Limit = 10;
            if (command.Limit > 50) command.Limit = 50;

            return await _movieApiService.SearchMoviesAsync(command);
        }
        catch (Exception ex)
        {
            return Result.Failure<SearchMovieResult>($"Search failed: {ex.Message}");
        }
    }
}
