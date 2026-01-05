using System.Threading.Tasks;
using Vidora.Core.Contracts.Results;
using Vidora.Core.Interfaces.Api;
using AutoMapper;
using CSharpFunctionalExtensions;
using System;
using Vidora.Core.Contracts.Commands;
using Vidora.Core.Contracts.Services;
using Vidora.Core.Entities;
using Vidora.Core.ValueObjects;
using Vidora.Core.Contracts;


namespace Vidora.Core.UseCases;
public class GetProfileUseCase
{
    private readonly IAuthApiService _authApiService;

  
    public GetProfileUseCase(IAuthApiService authApiService)
    {
        _authApiService = authApiService;
    }

    public async Task<Result<UserProfileResult>> ExecuteAsync()
    {
     
        var profileData = await _authApiService.GetProfileAsync();

     
        if (profileData.IsFailure)
        {
            return Result.Failure<UserProfileResult>(profileData.Error);
        }

        return Result.Success<UserProfileResult>(profileData.Value);
    }
}