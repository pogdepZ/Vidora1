using System;

namespace Vidora.Infrastructure.Api.Dtos.Responses.Datas;

internal record PromoData(
    int DiscountId,
    string Code,
    string DiscountType,
    decimal Value,
    decimal MinOrderValue,
    decimal? MaxDiscount, 
    DateTime EndDate,
    bool IsUsed
);
