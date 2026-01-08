using System;

namespace Vidora.Infrastructure.Api.Dtos.Responses.Datas;

internal record PlanData(
    int PlanId,
    string Name,
    decimal Price,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    bool IsActive,
    int LeftDay
);