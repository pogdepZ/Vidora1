using CSharpFunctionalExtensions;
using System.Threading.Tasks;
using Vidora.Core.Contracts.Services;
using Vidora.Core.Events;
using Vidora.Core.Interfaces.Api;
using Vidora.Core.Interfaces.Storage;

namespace Vidora.Core.UseCases;

public class AutoLoginUseCase
{
    private readonly IAuthApiService _authService;
    private readonly ISessionStateService _sessionState;
    private readonly ISessionStorageService _sessionStorage;

    public AutoLoginUseCase(
        IAuthApiService authService,
        ISessionStateService sessionState,
        ISessionStorageService sessionStorage)
    {
        _authService = authService;
        _sessionState = sessionState; 
        _sessionStorage = sessionStorage;
    }

    public async Task<Result> ExecuteAsync()
    {
        _sessionState.RestoreSession();
        if (!_sessionState.IsSessionValid)
        {
            _sessionState.ClearSession(SessionChangeReason.ForcedLogout);
            return Result.Failure("No valid session found. Please log in.");
        }

        if (_sessionState.CurrentSession.AccessToken.IsExpired)
        {
            var accessTokenResult = await _authService.RefreshTokenAsync(_sessionState.RefreshToken.Token);
            if (accessTokenResult.IsFailure)
            {
                _sessionState.ClearSession(SessionChangeReason.SessionExpired);
                return Result.Failure("Session expired. Please log in again.");
            }
            else
            {
                _sessionState.UpdateAccessToken(accessTokenResult.Value);
            }
        }

        return Result.Success("Auto login successful.");
    }
}
