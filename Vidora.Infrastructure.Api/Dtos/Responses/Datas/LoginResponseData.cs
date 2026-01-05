namespace Vidora.Infrastructure.Api.Dtos.Responses.Datas;

internal record LoginResponseData(
    UserData User,
    string AccessToken,
    string ExpiresIn
);
