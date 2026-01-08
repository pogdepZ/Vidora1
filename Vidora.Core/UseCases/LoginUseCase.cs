using AutoMapper;
using System;
using System.Threading.Tasks;
using Vidora.Core.Contracts.Commands;
using Vidora.Core.Contracts.Results;
using Vidora.Core.Contracts.Services;
using Vidora.Core.Entities;
using Vidora.Core.Exceptions;
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

    public Task<LoginResult> ExecuteAsync(LoginCommand command)
    {
        try
        {
            return ExecuteAsyncInternal(command);
        }
        catch (UnauthorizationException)
        {
            throw;
        }
        catch (DomainException)
        {
            throw;
        }
        catch(Exception ex)
        {
            throw new DomainException(ex.Message);
        }
    }

    private async Task<LoginResult> ExecuteAsyncInternal(LoginCommand command)
    {
        // Validate
        var apiRequest = command with
        {
            Email = new Email(command.Email),
            Password = new Password(command.Password)
        };

        // Call api
        var loginResult = await _authApiService.LoginAsync(apiRequest);

        // Save Sesion
        var mapped = _mapper.Map<Session>(loginResult);

        var session = new Session
        {
            CurrentUser = mapped.CurrentUser,
            AccessToken = new AuthToken(
                mapped.AccessToken.Token,
                DateTime.UtcNow.AddDays(100)
            )
        };

        _sessionState.SetSession(session, Events.SessionChangeReason.ManualLogin);

        // Return
        return loginResult;
    }
}
