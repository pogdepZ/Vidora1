using AutoMapper;
using CSharpFunctionalExtensions;
using System;
using System.Threading.Tasks;
using Vidora.Core.Contracts.Services;
using Vidora.Core.Dtos.Requests;
using Vidora.Core.Dtos.Responses;
using Vidora.Core.Entities;
using Vidora.Core.Interfaces.Api;
using Vidora.Core.ValueObjects;

namespace Vidora.Core.UseCases;

public class LoginUseCase
{
    private readonly IAuthApiService _authService;
    private readonly ISessionStateService _sessionState;
    private readonly IMapper _mapper;
    public LoginUseCase(IAuthApiService authService, ISessionStateService sessionState, IMapper mapper)
    {
        _authService = authService;
        _sessionState = sessionState;
        _mapper = mapper;
    }

    public async Task<Result<LoginResponseDto>> ExecuteAsync(LoginRequestDto request)
    {
        try
        {
            return await ExecuteAsyncInternal(request);
        }
        catch (Exception ex)
        {
            return Result.Failure<LoginResponseDto>($"Login failed: {ex.Message}");
        }
    }


    private async Task<Result<LoginResponseDto>> ExecuteAsyncInternal(LoginRequestDto request)
    {
        // Validate
        var apiRequest = request with {
            Email = new Email(request.Email),
            Password = new Password(request.Password)
        };

        // Call api
        var apiResponse = await _authService.LoginAsync(apiRequest);
        if (apiResponse.IsFailure)
        {
            return Result.Failure<LoginResponseDto>(apiResponse.Error);
        }

        // Save Sesion
        var newSession = _mapper.Map<Session>(apiResponse.Value);
        _sessionState.SetSession(newSession);

        // Return
        return Result.Success<LoginResponseDto>(apiResponse.Value);
    }
}
