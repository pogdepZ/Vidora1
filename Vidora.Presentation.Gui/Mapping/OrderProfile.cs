using AutoMapper;

namespace Vidora.Presentation.Gui.Mapping;

public class OrderProfile : Profile
{
    public OrderProfile()
    {
        CreateMap<Core.Entities.Order, Models.Order>();
        CreateMap<Core.Entities.Promo, Models.Promo>();
    }
}
