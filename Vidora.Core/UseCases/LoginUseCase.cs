using AutoMapper;
using CSharpFunctionalExtensions;
using System.Threading.Tasks;
using Vidora.Core.Contracts.Requests;
using Vidora.Core.Contracts.Responses;
using Vidora.Core.Entities;
using Vidora.Core.Interfaces.Api;
using Vidora.Core.ValueObjects;

namespace Vidora.Core.UseCases;

public class LoginUseCase
{
    private readonly IAuthApiService _authService;
    private readonly IMapper _mapper;
    public LoginUseCase(IAuthApiService authService, IMapper mapper)
    {
        _authService = authService;
        _mapper = mapper;
    }

    public async Task<Result<LoginResponse>> ExecuteAsync(LoginRequest request)
    {
        // Validate
        if (!Email.TryCreate(request.Email, out var email, out var error))
        {
            return Result.Failure<LoginResponse>(error);
        }
        if (!Password.TryCreate(request.Password, out var password, out error))
        {
            return Result.Failure<LoginResponse>(error);
        }

        var apiRequest = new LoginRequest(email, password);

        // Call api
        var apiResponse = await _authService.LoginAsync(apiRequest);

        // Save Sesion
        var newSession = _mapper.Map<Session>(apiResponse.Value);

        // Return
        return Result.Success<LoginResponse>(apiResponse.Value);
    }
}
