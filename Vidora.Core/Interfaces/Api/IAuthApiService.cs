using CSharpFunctionalExtensions;
using System.Threading.Tasks;
using Vidora.Core.Contracts.Commands;
using Vidora.Core.Contracts.Results;

namespace Vidora.Core.Interfaces.Api;

public interface IAuthApiService
{
    Task<Result<LoginResult>> LoginAsync(LoginCommand command);

    Task<Result<RegisterResult>> RegisterAsync(RegisterCommand command);
}