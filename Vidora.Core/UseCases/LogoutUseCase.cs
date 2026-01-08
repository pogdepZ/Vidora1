using System;
using System.Threading.Tasks;
using Vidora.Core.Contracts.Services;
using Vidora.Core.Events;
using Vidora.Core.Exceptions;

namespace Vidora.Core.UseCases;

public class LogoutUseCase
{
    private readonly ISessionStateService _sessionStateService;

    public LogoutUseCase(ISessionStateService sessionStateService)
    {
        _sessionStateService = sessionStateService;
    }

    public Task ExecuteAsync()
    {
        try
        {
            return ExecuteAsyncInternal();
        }
        catch (UnauthorizationException)
        {
            throw;
        }
        catch (DomainException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new DomainException(ex.Message);
        }
    }

    private Task ExecuteAsyncInternal()
    {
        _sessionStateService.ClearSession(SessionChangeReason.ManualLogout);
        return Task.CompletedTask;
    }
}
