using CSharpFunctionalExtensions;
using System;
using System.Threading.Tasks;
using Vidora.Core.Dtos.Requests;
using Vidora.Core.Dtos.Responses;
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

    public async Task<Result<RegisterResponseDto>> ExecuteAsync(RegisterRequestDto request)
    {
        try
        {
            return await ExecuteAsyncInternal(request);
        }
        catch (Exception ex)
        {
            return Result.Failure<RegisterResponseDto>($"Registration failed: {ex.Message}");
        }
    }

    private async Task<Result<RegisterResponseDto>> ExecuteAsyncInternal(RegisterRequestDto request)
    {
        // Validate
        if (!Email.TryCreate(request.Email, out var validatedEmail, out var error))
        {
            return Result.Failure<RegisterResponseDto>(error);
        }
        if (!Password.TryCreate(request.Password, out var validatedPassword, out error))
        {
            return Result.Failure<RegisterResponseDto>(error);
        }

        var apiRequest = request with {
            Email = validatedEmail,
            Password = validatedPassword
        };

        var apiResponse = await _authApiService.RegisterAsync(apiRequest);
        if (apiResponse.IsFailure)
        {
            return Result.Failure<RegisterResponseDto>(apiResponse.Error);
        }

        return Result.Success<RegisterResponseDto>(apiResponse.Value);
    }
}
