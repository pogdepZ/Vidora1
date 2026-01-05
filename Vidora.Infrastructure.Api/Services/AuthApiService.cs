using AutoMapper;
using CSharpFunctionalExtensions;
using System;
using System.Threading.Tasks;
using Vidora.Core.Contracts.Commands;
using Vidora.Core.Contracts.Results;
using Vidora.Core.Interfaces.Api;
using Vidora.Infrastructure.Api.Dtos.Requests;
using Vidora.Infrastructure.Api.Dtos.Responses;
using Vidora.Infrastructure.Api.Dtos.Responses.Datas;
using Vidora.Infrastructure.Api.Extensions;

namespace Vidora.Infrastructure.Api.Services;

using LoginSuccessResponse = SuccessResponse<LoginResponseData>;
using RegisterSuccessResponse = SuccessResponse<RegisterResponseData>;

public class AuthApiService : IAuthApiService
{
    private readonly ApiClient _apiClient;
    private readonly IMapper _mapper;
    public AuthApiService(ApiClient apiClient, IMapper mapper)
    {
        _apiClient = apiClient;
        _mapper = mapper;
    }

    public async Task<Result<LoginResult>> LoginAsync(LoginCommand command)
    {
        var req = _mapper.Map<LoginRequest>(command);

        var httpRes = await _apiClient.PostAsync(
            url: "api/auth/login",
            body: req
            );

        var apiRes = await httpRes.ReadAsync<LoginResponseData>();
          
        if (apiRes is not LoginSuccessResponse success)
        {
            return Result.Failure<LoginResult>(
                apiRes.Message ?? "Login failed"
            );
        }

        var expiresAt = DateTime.UtcNow.Add(
            ParseExpiresIn(success.Data.ExpiresIn)
        );

        var result = _mapper.Map<LoginResult>(success.Data) with {
            ExpiresAt = expiresAt
        };

        return Result.Success(result);
    }


    public async Task<Result<RegisterResult>> RegisterAsync(RegisterCommand command)
    {
        var req = _mapper.Map<RegisterRequest>(command);

        var httpRes = await _apiClient.PostAsync(
            url: "api/auth/register",
            body: req
        );

        var apiRes = await httpRes.ReadAsync<RegisterResponseData>();

        if (apiRes is not RegisterSuccessResponse success)
        {
            return Result.Failure<RegisterResult>(
                apiRes.Message ?? "Register failed"
            );
        }

        var result = _mapper.Map<RegisterResult>(success.Data);

        return Result.Success(result);
    }

    private static TimeSpan ParseExpiresIn(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return TimeSpan.Zero;

        raw = raw.Trim().ToLower();

        if (raw.EndsWith('m') && int.TryParse(raw[..^1], out var minutes))
        {
            return TimeSpan.FromMinutes(minutes);
        }

        if (raw.EndsWith('h') && int.TryParse(raw[..^1], out var hours))
            return TimeSpan.FromHours(hours);

        if (int.TryParse(raw, out var seconds))
            return TimeSpan.FromSeconds(seconds);

        if (TimeSpan.TryParse(raw, out var span))
            return span;

        return TimeSpan.Zero;
    }
}
