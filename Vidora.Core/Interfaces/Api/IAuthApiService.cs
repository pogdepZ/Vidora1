using CSharpFunctionalExtensions;
using System.Threading.Tasks;
using Vidora.Core.Dtos.Requests;
using Vidora.Core.Dtos.Responses;

namespace Vidora.Core.Interfaces.Api;

public interface IAuthApiService
{
    Task<Result<LoginResponseDto>> LoginAsync(LoginRequestDto request);

    Task<Result<RegisterResponseDto>> RegisterAsync(RegisterRequestDto request);
}