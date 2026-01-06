using AutoMapper;
using CSharpFunctionalExtensions;
using System;
using System.Threading.Tasks;
using Vidora.Core.Contracts.Commands;
using Vidora.Core.Contracts.Results;
using Vidora.Core.Contracts.Services;
using Vidora.Core.Entities;
using Vidora.Core.Interfaces.Api;
using Vidora.Core.ValueObjects;

namespace Vidora.Core.UseCases;

public class LoginUseCase
{
    private readonly IAuthApiService _authApiService;
    private readonly ISessionStateService _sessionState;
    private readonly IMapper _mapper;
    public LoginUseCase(IAuthApiService authService, ISessionStateService sessionState, IMapper mapper)
    {
        _authApiService = authService;
        _sessionState = sessionState;
        _mapper = mapper;
    }

    public async Task<Result<LoginResult>> ExecuteAsync(LoginCommand command)
    {
        try
        {
            return await ExecuteAsyncInternal(command);
        }
        catch (Exception ex)
        {
            return Result.Failure<LoginResult>($"Login failed: {ex.Message}");
        }
    }


    private async Task<Result<LoginResult>> ExecuteAsyncInternal(LoginCommand command)
    {
        // Validate
        var apiRequest = command with {
            Email = new Email(command.Email),
            Password = new Password(command.Password)
        };

        // Call api
        var apiResponse = await _authApiService.LoginAsync(apiRequest);
        if (apiResponse.IsFailure)
        {
            return Result.Failure<LoginResult>(apiResponse.Error);
        }

        // Save Sesion
        var session = _mapper.Map<Session>(apiResponse.Value);
        _sessionState.SetSession(session, Events.SessionChangeReason.ManualLogin);

        // Return
        return Result.Success<LoginResult>(apiResponse.Value);
    }
}
