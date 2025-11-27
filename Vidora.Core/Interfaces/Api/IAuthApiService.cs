using CSharpFunctionalExtensions;
using System.Threading.Tasks;
using Vidora.Core.Contracts.Requests;
using Vidora.Core.Contracts.Responses;
using Vidora.Core.ValueObjects;

namespace Vidora.Core.Interfaces.Api;

public interface IAuthApiService
{
    Task<Result<LoginResponse>> LoginAsync(LoginRequest request);

    Task<Result<RegisterResponse>> RegisterAsync(RegisterRequest request);

    Task<Result<AuthToken>> RefreshTokenAsync(string refreshToken);
}