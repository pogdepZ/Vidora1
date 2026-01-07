using AutoMapper;
using CSharpFunctionalExtensions;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Web;
using Vidora.Core.Contracts.Commands;
using Vidora.Core.Contracts.Results;
using Vidora.Core.Contracts.Services;
using Vidora.Core.Entities;
using Vidora.Core.Interfaces.Api;
using Vidora.Infrastructure.Api.Dtos.Responses;
using Vidora.Infrastructure.Api.Dtos.Responses.Datas;
using Vidora.Infrastructure.Api.Extensions;

namespace Vidora.Infrastructure.Api.Services;

public class MovieApiService : IMovieApiService
{
    private readonly ApiClient _apiClient;
    private readonly IMapper _mapper;
    private readonly ISessionStateService _sessionService;

    public MovieApiService(ApiClient apiClient, IMapper mapper, ISessionStateService sessionService)
    {
        _apiClient = apiClient;
        _mapper = mapper;
        _sessionService = sessionService;
    }

    public async Task<Result<SearchMovieResult>> SearchMoviesAsync(SearchMovieCommand command)
    {
        // Get access token
        var tokenObject = _sessionService.AccessToken;
        var accessToken = tokenObject?.Token;

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return Result.Failure<SearchMovieResult>("Access token not found. Please login again.");
        }

        // Build query string
        var queryParams = HttpUtility.ParseQueryString(string.Empty);
        
        if (!string.IsNullOrWhiteSpace(command.Title))
            queryParams["title"] = command.Title;
        
        if (command.GenreId.HasValue)
            queryParams["genreId"] = command.GenreId.Value.ToString();
        
        if (command.ReleaseYear.HasValue)
            queryParams["releaseYear"] = command.ReleaseYear.Value.ToString();
        
        queryParams["page"] = command.Page.ToString();
        queryParams["limit"] = command.Limit.ToString();

        var url = $"api/movies?{queryParams}";

        var httpRes = await _apiClient.GetAsync(url, token: accessToken);

        var apiRes = await httpRes.ReadPaginatedAsync<MovieData>();

        if (apiRes is not PaginatedSucessResponse<MovieData> success)
        {
            return Result.Failure<SearchMovieResult>(
                apiRes.Message ?? "Tìm kiếm phim thất bại"
            );
        }

        var result = new SearchMovieResult
        {
            Movies = _mapper.Map<IReadOnlyList<Movie>>(success.Data),
            Pagination = _mapper.Map<PaginationResult>(success.Pagination)
        };

        return Result.Success(result);
    }

    public async Task<Result<MovieDetailResult>> GetMovieDetailAsync(int movieId)
    {
        // Get access token
        var tokenObject = _sessionService.AccessToken;
        var accessToken = tokenObject?.Token;

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return Result.Failure<MovieDetailResult>("Access token not found. Please login again.");
        }

        var url = $"api/movies/{movieId}";

        var httpRes = await _apiClient.GetAsync(url, token: accessToken);

        var apiRes = await httpRes.ReadAsync<MovieData>();

        if (apiRes is not SuccessResponse<MovieData> success)
        {
            return Result.Failure<MovieDetailResult>(
                apiRes.Message ?? "Lấy thông tin phim thất bại"
            );
        }

        var result = new MovieDetailResult
        {
            Movie = _mapper.Map<Movie>(success.Data)
        };

        return Result.Success(result);
    }

    public async Task<Result<GenreListResult>> GetGenresAsync()
    {
        // Get access token
        var tokenObject = _sessionService.AccessToken;
        var accessToken = tokenObject?.Token;

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return Result.Failure<GenreListResult>("Access token not found. Please login again.");
        }

        var url = "api/movies/genres";

        var httpRes = await _apiClient.GetAsync(url, token: accessToken);

        var apiRes = await httpRes.ReadListAsync<GenreData>();

        if (apiRes is not ListSuccessResponse<GenreData> success)
        {
            return Result.Failure<GenreListResult>(
                apiRes.Message ?? "Lấy danh sách thể loại thất bại"
            );
        }

        var result = new GenreListResult
        {
            Genres = _mapper.Map<IReadOnlyList<Genre>>(success.Data)
        };

        return Result.Success(result);
    }

    public async Task<Result<RateMovieResult>> RateMovieAsync(int movieId, int rating)
    {
        var tokenObject = _sessionService.AccessToken;
        var accessToken = tokenObject?.Token;

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return Result.Failure<RateMovieResult>("Access token not found. Please login again.");
        }

        if (rating < 1 || rating > 10)
        {
            return Result.Failure<RateMovieResult>("Đánh giá phải từ 1 đến 10 sao");
        }

        var url = $"api/movies/{movieId}/rating";
        var body = new { number = rating };

        var httpRes = await _apiClient.PutAsync(url, body, token: accessToken);

        if (httpRes.IsSuccessStatusCode)
        {
            return Result.Success(new RateMovieResult
            {
                Success = true,
                Message = $"Đã đánh giá {rating} sao",
                UserRating = rating
            });
        }

        var apiRes = await httpRes.ReadAsync<object>();
        return Result.Failure<RateMovieResult>(
            apiRes.Message ?? "Đánh giá phim thất bại"
        );
    }
}
