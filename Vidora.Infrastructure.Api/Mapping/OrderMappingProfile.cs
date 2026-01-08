using AutoMapper;
using Vidora.Core.Entities;
using Vidora.Infrastructure.Api.Dtos.Responses.Datas;

namespace Vidora.Infrastructure.Api.Mapping;

public class OrderMappingProfile : Profile
{
    public OrderMappingProfile()
    {
        CreateMap<OrderData, Order>();
        CreateMap<PromoData, Promo>();
    }
}
