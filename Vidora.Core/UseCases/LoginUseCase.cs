using AutoMapper;
using CSharpFunctionalExtensions;
using System.Threading.Tasks;
using Vidora.Core.Contracts.Requests;
using Vidora.Core.Contracts.Responses;
using Vidora.Core.Contracts.Services;
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

    public async Task<Result<LoginResponse>> ExecuteAsync(LoginRequest request)
    {
        // Validate
        if (!Email.TryCreate(request.Email, out var validatedEmail, out var error))
        {
            return Result.Failure<LoginResponse>(error);
        }
        if (!Password.TryCreate(request.Password, out var validatedPassword, out error))
        {
            return Result.Failure<LoginResponse>(error);
        }

        var apiRequest = request with {
            Email = validatedEmail,
            Password = validatedPassword
        };

        // Call api
        var apiResponse = await _authService.LoginAsync(apiRequest);
        if (apiResponse.IsFailure)
        {
            return Result.Failure<LoginResponse>(apiResponse.Error);
        }

        // Save Sesion
        var newSession = _mapper.Map<Session>(apiResponse.Value);
        _sessionState.SetSession(newSession);

        // Return
        return Result.Success<LoginResponse>(apiResponse.Value);
    }
}
