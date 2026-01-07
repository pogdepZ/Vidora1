using CSharpFunctionalExtensions;
using System.Threading.Tasks;
using Vidora.Core.Contracts.Commands;
using Vidora.Core.Contracts.Results;

namespace Vidora.Core.Interfaces.Api;

public interface IAuthApiService
{
    Task<LoginResult> LoginAsync(LoginCommand command);

    Task<RegisterResult> RegisterAsync(RegisterCommand command);

    Task<Result<UserProfileResult>> GetProfileAsync();
     
    Task<Result<UserProfileResult>> UpdateProfileAsync(UpdateProfileCommand command);

}