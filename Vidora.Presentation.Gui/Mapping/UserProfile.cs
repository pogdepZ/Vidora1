using AutoMapper;

namespace Vidora.Presentation.Gui.Mapping;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<Core.Entities.Role, Models.Role>()
            .ConvertUsing(src => (Models.Role)src);
    }
}
