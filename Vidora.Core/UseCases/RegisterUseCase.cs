using CSharpFunctionalExtensions;
using System;
using System.Threading.Tasks;
using Vidora.Core.Contracts.Commands;
using Vidora.Core.Contracts.Results;
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

    public async Task<Result<RegisterResult>> ExecuteAsync(RegisterCommand command)
    {
        try
        {
            return await ExecuteAsyncInternal(command);
        }
        catch (Exception ex)
        {
            return Result.Failure<RegisterResult>($"Registration failed: {ex.Message}");
        }
    }

    private async Task<Result<RegisterResult>> ExecuteAsyncInternal(RegisterCommand command)
    {
        // Validate
        var apiRequest = command with
        {
            Email = new Email(command.Email),
            Password = new Password(command.Password)
        };

        var apiResponse = await _authApiService.RegisterAsync(apiRequest);
        if (apiResponse.IsFailure)
        {
            return Result.Failure<RegisterResult>(apiResponse.Error);
        }
        
        return Result.Success<RegisterResult>(apiResponse.Value);
    }
}
