using CSharpFunctionalExtensions;
using System;
using System.Threading.Tasks;
using Vidora.Core.Contracts.Results;
using Vidora.Core.Interfaces.Api;

namespace Vidora.Core.UseCases;

public class RateMovieUseCase
{
    private readonly IMovieApiService _movieApiService;

    public RateMovieUseCase(IMovieApiService movieApiService)
    {
        _movieApiService = movieApiService;
    }

    public async Task<Result<RateMovieResult>> ExecuteAsync(int movieId, int rating)
    {
        try
        {
            if (movieId <= 0)
            {
                return Result.Failure<RateMovieResult>("ID phim không h?p l?");
            }

            if (rating < 1 || rating > 10)
            {
                return Result.Failure<RateMovieResult>("Ðánh giá ph?i t? 1 ð?n 10 sao");
            }

            return await _movieApiService.RateMovieAsync(movieId, rating);
        }
        catch (Exception ex)
        {
            return Result.Failure<RateMovieResult>($"L?i ðánh giá phim: {ex.Message}");
        }
    }
}