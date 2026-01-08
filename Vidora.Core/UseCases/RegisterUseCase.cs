using System;
using System.Threading.Tasks;
using Vidora.Core.Contracts.Commands;
using Vidora.Core.Contracts.Results;
using Vidora.Core.Exceptions;
using Vidora.Core.Interfaces.Api;
using Vidora.Core.ValueObjects;

namespace Vidora.Core.UseCases;

public class RegisterUseCase
{
    private readonly IAuthApiService _authApiService;
    public RegisterUseCase(IAuthApiService authApiService)
    {
        _authApiService = authApiService;
    }

    public Task<RegisterResult> ExecuteAsync(RegisterCommand command)
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
        catch (Exception ex)
        {
            throw new DomainException(ex.Message);
        }
    }

    private Task<RegisterResult> ExecuteAsyncInternal(RegisterCommand command)
    {
        // Validate
        var apiRequest = command with
        {
            Email = new Email(command.Email),
            Password = new Password(command.Password)
        };

        return _authApiService.RegisterAsync(apiRequest);
    }
}
