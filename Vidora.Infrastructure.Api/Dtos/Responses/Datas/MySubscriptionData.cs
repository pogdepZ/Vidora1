using System.Text.Json.Serialization;

namespace Vidora.Infrastructure.Api.Dtos.Responses.Datas;

internal record MySubscriptionData(
    [property: JsonPropertyName("plan_id")] int PlanId,
    [property: JsonPropertyName("is_active")] bool IsActive
);
