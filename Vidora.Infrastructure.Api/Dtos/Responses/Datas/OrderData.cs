using System.Text.Json.Serialization;

namespace Vidora.Infrastructure.Api.Dtos.Responses.Datas;

internal record OrderData(
    int OrderId,
    string OrderCode,
    int PlanId,
    string PlanName,
    string OrderType,
    int OriginalAmount,
    int DiscountAmount,
    string DiscountCode,
    int FinalAmount,
    string Status
);
