using CSharpFunctionalExtensions;
using System.Diagnostics;
using System.Threading.Tasks;
using Vidora.Core.Contracts;
using Vidora.Core.Contracts.Commands;
using Vidora.Core.Contracts.Results;
using Vidora.Core.Contracts.Services;
using Vidora.Core.Interfaces.Api;

namespace Vidora.Core.UseCases;

public class UpdateProfileUseCase
{
    private readonly IAuthApiService _authApiService;
    private readonly ISessionStateService _sessionStateService;

    public UpdateProfileUseCase(IAuthApiService authApiService, ISessionStateService sessionStateService)
    {
        _authApiService = authApiService;
        _sessionStateService = sessionStateService;
    }

    public async Task<Result<UserProfileResult>> ExecuteAsync(UpdateProfileCommand command)
    {
        // Kiểm tra session trước khi gọi API
        if (!_sessionStateService.IsAuthenticated)
        {
            return Result.Failure<UserProfileResult>("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.");
        }

        var updateResult = await _authApiService.UpdateProfileAsync(command);

        if (updateResult.IsFailure)
        {
            return Result.Failure<UserProfileResult>(updateResult.Error);
        }


        Debug.Assert(updateResult.Value != null, "UpdateProfileAsync returned success but Value is null");
        _sessionStateService.UpdateUser(updateResult.Value.User);
        return Result.Success(updateResult.Value);
    }
}