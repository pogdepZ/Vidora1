namespace Vidora.Infrastructure.Api.Dtos.Responses.Datas;

internal record ProfileDataResponse (
    UserData User,
    PlanData? CurrentPlan
);