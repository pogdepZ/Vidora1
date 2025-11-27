using CSharpFunctionalExtensions;
using System.Threading.Tasks;
using Vidora.Core.Contracts.Requests;
using Vidora.Core.Contracts.Responses;

namespace Vidora.Core.Interfaces.Api;

public interface IAuthApiService
{
    Task<Result<LoginResponse>> LoginAsync(LoginRequest request);
}