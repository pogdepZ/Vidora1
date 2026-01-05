using AutoMapper;
using Vidora.Core.Contracts.Results;
using Vidora.Core.Entities;
using Vidora.Core.Extensions;
using Vidora.Core.ValueObjects;

namespace Vidora.Core.Mapping;

public class LoginMappingProfile : Profile
{
    public LoginMappingProfile()
    {
        // Result -> Entities
        CreateMap<UserResult, User>()
            .ForMember(d => d.Email, o => o.MapFrom(s => new Email(s.Email)))
            .ForMember(d => d.Role, o => o.MapFrom(s => ParseRole(s.Role)))
            .ForMember(d => d.Gender, o => o.MapFrom(s => ParseGender(s.Gender)))
            .ForMember(d => d.Status, o => o.MapFrom(s => ParseStatus(s.Status)));

        CreateMap<LoginResult, Session>()
            .ForMember(d => d.CurrentUser, o => o.MapFrom(s => s.User))
            .ForMember(d => d.AccessToken, o => o.MapFrom(s => new AuthToken(s.AccessToken, s.ExpiresAt)));

        // Entities -> Result

    }

    private static Role ParseRole(string role) => role.ToEnum<Role>(Role.User);
    private static Gender? ParseGender(string? gender) => gender.ToEnum<Gender>();
    private static Status? ParseStatus(string? status) => status.ToEnum<Status>();
}
