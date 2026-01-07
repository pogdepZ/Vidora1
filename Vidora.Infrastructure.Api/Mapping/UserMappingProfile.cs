using AutoMapper;
using System;
using Vidora.Core.Contracts.Commands;
using Vidora.Core.Contracts.Results;
using Vidora.Infrastructure.Api.Dtos.Requests;
using Vidora.Infrastructure.Api.Dtos.Responses.Datas;

namespace Vidora.Infrastructure.Api.Mapping;

internal class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        // Command -> Request

        // Response -> Result
        CreateMap<UserData, UserResult>();
    }
}
