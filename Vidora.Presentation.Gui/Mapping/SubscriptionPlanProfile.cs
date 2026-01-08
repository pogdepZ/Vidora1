using AutoMapper;

namespace Vidora.Presentation.Gui.Mapping;

public class SubscriptionPlanProfile : Profile
{
    public SubscriptionPlanProfile()
    {
        CreateMap<Core.Entities.SubscriptionPlan, Models.SubscriptionPlan>()
            .ForMember(dest => dest.IsPurchased, opt => opt.Ignore());
    }
}
