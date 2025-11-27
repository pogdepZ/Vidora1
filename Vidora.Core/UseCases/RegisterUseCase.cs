using CSharpFunctionalExtensions;
using System.Threading.Tasks;
using Vidora.Core.Contracts.Requests;
using Vidora.Core.Contracts.Responses;
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

    public async Task<Result<RegisterResponse>> ExecuteAsync(RegisterRequest request)
    {
        // Validate
        if (!Email.TryCreate(request.Email, out var validatedEmail, out var error))
        {
            return Result.Failure<RegisterResponse>(error);
        }
        if (!Password.TryCreate(request.Password, out var validatedPassword, out error))
        {
            return Result.Failure<RegisterResponse>(error);
        }

        var apiRequest = request with {
            Email = validatedEmail,
            Password = validatedPassword
        };

        var apiResponse = await _authApiService.RegisterAsync(apiRequest);
        if (apiResponse.IsFailure)
        {
            return Result.Failure<RegisterResponse>(apiResponse.Error);
        }

        return Result.Success<RegisterResponse>(apiResponse.Value);
    }
}
