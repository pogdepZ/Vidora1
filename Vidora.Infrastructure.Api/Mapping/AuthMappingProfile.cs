using AutoMapper;
using System;
using Vidora.Core.Contracts.Commands;
using Vidora.Core.Contracts.Results;
using Vidora.Infrastructure.Api.Dtos.Requests;
using Vidora.Infrastructure.Api.Dtos.Responses.Datas;

namespace Vidora.Infrastructure.Api.Mapping;

internal class AuthMappingProfile : Profile
{
    public AuthMappingProfile()
    {
        // Command -> Request
        CreateMap<LoginCommand, LoginRequest>();
        CreateMap<RegisterCommand, RegisterRequest>();

        // LoginResponseData -> LoginResult
        CreateMap<LoginResponseData, LoginResult>()
            .ForCtorParam(nameof(LoginResult.ExpiresAt), o => o.MapFrom(_ => DateTime.MinValue));

        // RegisterResponseData -> RegisterResult
        CreateMap<RegisterResponseData, RegisterResult>()
            .ForCtorParam(nameof(RegisterResult.Message), o => o.MapFrom(_ => "Registration successfully"));
    }
}
