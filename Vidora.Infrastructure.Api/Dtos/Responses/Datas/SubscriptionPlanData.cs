namespace Vidora.Infrastructure.Api.Dtos.Responses.Datas;

internal record SubscriptionPlanData(
    int PlanId,
    string Name,
    decimal Price,
    int Durations,
    string Description
);
