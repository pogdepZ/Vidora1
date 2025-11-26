using AutoMapper;
using System;
using Vidora.Core.Contracts.Responses;
using Vidora.Core.Entities;
using Vidora.Core.ValueObjects;

namespace Vidora.Core.Mapping;

public class LoginProfile : Profile
{
    public LoginProfile()
    {
        CreateMap<LoginResponse, User>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => new Email(src.Email)))
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => ParseRole(src.Role)));

        CreateMap<LoginResponse, Session>()
            .ForMember(dest => dest.CurrentUser, opt => opt.MapFrom(src => src))
            .ForMember(dest => dest.AccessToken, opt => opt.MapFrom(src => new AuthToken(src.AccessToken, src.ExpiresAt)))
            .ForMember(dest => dest.RefreshToken, opt => opt.MapFrom(src => new AuthToken(src.RefreshToken, src.RefreshExpiresAt)));
    }

    private static Role ParseRole(string roleStr)
    {
        return Enum.TryParse<Role>(roleStr, ignoreCase: true, out var role) ? role : Role.User;
    }
}
