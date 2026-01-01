using CSharpFunctionalExtensions;
using System;
using System.Threading.Tasks;
using Vidora.Core.Dtos.Requests;
using Vidora.Core.Dtos.Responses;
using Vidora.Core.Interfaces.Api;

namespace Vidora.Infrastructure.Api.Services;

public class AuthApiService : IAuthApiService
{
    public AuthApiService()
    {
    }

    public async Task<Result<LoginResponseDto>> LoginAsync(LoginRequestDto request)
    {
        // TODO: add logic
        var id = Guid.NewGuid().ToString();
        var email = request.Email;
        var role = "User";
        var accessToken = Guid.NewGuid().ToString();
        var expiresIn = TimeSpan.FromMinutes(15);
        var refreshToken = Guid.NewGuid().ToString();
        var refreshExpiresIn = TimeSpan.FromDays(1);

        return new LoginResponseDto(
            UserId: id,
            Email: email,
            Role: role,
            AccessToken: accessToken,
            ExpiresAt: DateTime.UtcNow.Add(expiresIn),
            RefreshToken: refreshToken,
            RefreshExpiresAt: DateTime.UtcNow.Add(refreshExpiresIn)
            );
    }

    public async Task<Result<RegisterResponseDto>> RegisterAsync(RegisterRequestDto request)
    {
        await Task.Delay(500); // Simulate some processing time

        return new RegisterResponseDto(
            Message: "User registered successfully"
            );
    }
}
