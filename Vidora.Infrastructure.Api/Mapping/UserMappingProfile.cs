using AutoMapper;
using System;
using Vidora.Core.Contracts.Commands;
using Vidora.Core.Contracts.Results;
using Vidora.Core.Entities;
using Vidora.Core.ValueObjects;
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

        // Data Response -> Core Result
        CreateMap<ProfileDataResponse, UserProfileResult>();

        CreateMap<UserData, User>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => new Email(src.Email)))
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => ParseRole(src.Role)))
            .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => ParseEnum<Gender>(src.Gender)))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => ParseEnum<Status>(src.Status)));

        CreateMap<PlanData, Plan>()
            .ForMember(dest => dest.PlanId, opt => opt.MapFrom(src => src.PlanId))
            .ForMember(dest => dest.LeftDay, opt => opt.MapFrom(src => src.LeftDay));


        CreateMap<UpdateProfileCommand, UpdateProfileRequest>()
            .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender.HasValue ? src.Gender.Value.ToString().ToUpper() : null));

        CreateMap<UpdateProfileResponse, User>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => new Email(src.Email)))
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => ParseRole(src.Role)))
            .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => ParseEnum<Gender>(src.Gender)))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => ParseEnum<Status>(src.Status)));
        
        CreateMap<UpdateProfileResponse, UserProfileResult>()
            .ForMember(dest => dest.User, opt => opt.MapFrom(src => src))
            .ForMember(dest => dest.CurrentPlan, opt => opt.Ignore());
    }

    private static Role ParseRole(string roleStr)
    {
        return Enum.TryParse<Role>(roleStr, true, out var role) ? role : Role.User;
    }

    private static TEnum? ParseEnum<TEnum>(string? value) where TEnum : struct
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return Enum.TryParse<TEnum>(value, true, out var result) ? result : null;
    }
}
