using AutoMapper;
using Vidora.Core.Entities;
using Vidora.Infrastructure.Api.Dtos.Responses.Datas;

namespace Vidora.Infrastructure.Api.Mapping;

public class SubscriptionMappingProfile : Profile
{
    public SubscriptionMappingProfile()
    {
        CreateMap<SubscriptionPlanData, SubscriptionPlan>();
        CreateMap<MySubscriptionData, MySubscription>();
    }
}
