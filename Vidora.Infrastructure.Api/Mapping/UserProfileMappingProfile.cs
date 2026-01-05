using AutoMapper;
using System;
using Vidora.Core.Contracts.Commands;
using Vidora.Core.Contracts.Results;
using Vidora.Core.Entities;
using Vidora.Core.ValueObjects;
using Vidora.Infrastructure.Api.Dtos.Requests;
using Vidora.Infrastructure.Api.Dtos.Responses.Datas;

namespace Vidora.Infrastructure.Api.Mapping;

public class UserProfileMappingProfile : Profile
{
    public UserProfileMappingProfile()
    {
        // 1. Map lớp bao ngoài (Data Response -> Core Result)
        CreateMap<ProfileDataResponse, UserProfileResult>();

        // 2. Map UserData (DTO) -> User (Entity)
        CreateMap<UserData, User>()
            // Map ID
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))

            // --- QUAN TRỌNG: Map string Email sang Value Object Email ---
            // Vì Entity dùng class Email, còn DTO là string, phải new Email() thủ công
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => new Email(src.Email)))

            // Map Role (String -> Enum)
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => ParseRole(src.Role)))

            // Map Gender & Status (Nếu DTO trả về string/null)
            .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => ParseEnum<Gender>(src.Gender)))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => ParseEnum<Status>(src.Status)))

            // Các trường trùng tên (Username, FullName, Avatar...) AutoMapper tự map
            ;

        // 3. Map PlanData (DTO) -> Plan (Entity)
        CreateMap<PlanData, Plan>()
            .ForMember(dest => dest.PlanId, opt => opt.MapFrom(src => src.PlanId)) // Entity dùng Id hay PlanId? Check kỹ file Entity
            .ForMember(dest => dest.LeftDay, opt => opt.MapFrom(src => src.LeftDay)); // Map LeftDay -> RemainingDays


        // 4. Map UpdateProfileCommand -> UpdateProfileRequest
        CreateMap<UpdateProfileCommand, UpdateProfileRequest>()
            // Ví dụ: Map Enum Gender sang string
            .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender.HasValue ? src.Gender.ToString().ToLower() : null));

        // 5. Map UpdateProfileResponse -> User (Entity)
        CreateMap<UpdateProfileResponse, User>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => new Email(src.Email)))
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => ParseRole(src.Role)))
            .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => ParseEnum<Gender>(src.Gender)))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => ParseEnum<Status>(src.Status)));

        // 6. Map UpdateProfileResponse -> UserProfileResult (với CurrentPlan = null)
        CreateMap<UpdateProfileResponse, UserProfileResult>()
            .ForMember(dest => dest.User, opt => opt.MapFrom(src => src))
            .ForMember(dest => dest.CurrentPlan, opt => opt.Ignore());
    }

    // Helper riêng cho Role (có giá trị mặc định là User nếu lỗi)
    private static Role ParseRole(string roleStr)
    {
        return Enum.TryParse<Role>(roleStr, true, out var role) ? role : Role.User;
    }

    // Helper Generic cho các Enum khác (Gender, Status...) - Trả về null nếu parse lỗi
    private static TEnum? ParseEnum<TEnum>(string? value) where TEnum : struct
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return Enum.TryParse<TEnum>(value, true, out var result) ? result : null;
    }
}