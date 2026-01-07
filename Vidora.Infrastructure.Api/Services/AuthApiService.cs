using AutoMapper;
using CSharpFunctionalExtensions;
using System;
using System.Threading.Tasks;
using Vidora.Core.Contracts.Commands;
using Vidora.Core.Contracts.Results;
using Vidora.Core.Interfaces.Api;
using Vidora.Infrastructure.Api.Clients;
using Vidora.Infrastructure.Api.Dtos.Requests;
using Vidora.Infrastructure.Api.Dtos.Responses;
using Vidora.Infrastructure.Api.Dtos.Responses.Datas;
using Vidora.Infrastructure.Api.Extensions;

namespace Vidora.Infrastructure.Api.Services;

public class AuthApiService : IAuthApiService
{
    private readonly ApiClient _apiClient;
    private readonly IMapper _mapper;
    public AuthApiService(ApiClient apiClient, IMapper mapper)
    {
        _apiClient = apiClient;
        _mapper = mapper;
    }

    public async Task<LoginResult> LoginAsync(LoginCommand command)
    {
        var req = _mapper.Map<LoginRequest>(command);

        var httpRes = await _apiClient.PostAsync(
            path: "api/auth/login",
            body: req
            );

        var apiRes = await httpRes.ReadAsync<LoginResponseData>();
        var success = apiRes.EnsureSuccess<LoginResponseData>();

        var expiresAt = DateTime.UtcNow.Add(
            ParseExpiresIn(success.Data.ExpiresIn)
        );

        var result = _mapper.Map<LoginResult>(success.Data) with
        {
            ExpiresAt = expiresAt
        };

        return result;
    }


    public async Task<RegisterResult> RegisterAsync(RegisterCommand command)
    {
        var req = _mapper.Map<RegisterRequest>(command);

        var httpRes = await _apiClient.PostAsync(
            path: "api/auth/register",
            body: req
        );

        var apiRes = await httpRes.ReadAsync<RegisterResponseData>();
        var success = apiRes.EnsureSuccess<SuccessResponse<RegisterResponseData>>();

        var result = _mapper.Map<RegisterResult>(success.Data);

        return result;
    }


    public async Task<Result<UserProfileResult>> GetProfileAsync()
    {
        var httpRes = await _apiClient.GetAsync(
            path: "api/auth/me"            
        );

        var apiRes = await httpRes.ReadAsync<ProfileDataResponse>();

        if (apiRes is not SuccessResponse<ProfileDataResponse> success)
        {
            return Result.Failure<UserProfileResult>(
                apiRes.Message ?? "Lấy thông tin người dùng thất bại"
            );
        }

        var result = _mapper.Map<UserProfileResult>(success.Data);

        return Result.Success(result);
    }


    public async Task<Result<UserProfileResult>> UpdateProfileAsync(UpdateProfileCommand command)
    {
        var request = _mapper.Map<UpdateProfileRequest>(command);

        var httpRes = await _apiClient.PutAsync(
            url: "api/users/profile",
            body: request
        );

        var apiRes = await httpRes.ReadAsync<UpdateProfileResponse>();
        if (apiRes is not SuccessResponse<UpdateProfileResponse> success)
        {
            return Result.Failure<UserProfileResult>(
                apiRes.Message ?? "Cập nhật thông tin thất bại"
            );
        }

        var result = _mapper.Map<UserProfileResult>(success.Data);

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
