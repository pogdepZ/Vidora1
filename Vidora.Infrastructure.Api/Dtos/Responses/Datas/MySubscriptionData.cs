using System;
using System.Text.Json.Serialization;

namespace Vidora.Infrastructure.Api.Dtos.Responses.Datas;

internal record CurrentSubscriptionData(
    [property: JsonPropertyName("plan_id")] int PlanId,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("price")] decimal Price,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("start_date")] DateTime StartDate,
    [property: JsonPropertyName("end_date")] DateTime EndDate,
    [property: JsonPropertyName("is_active")] bool IsActive,
    [property: JsonPropertyName("left_day")] int LeftDay
);
