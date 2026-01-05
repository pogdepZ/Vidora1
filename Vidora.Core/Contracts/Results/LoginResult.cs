using System;

namespace Vidora.Core.Contracts.Results;

public record LoginResult(
    UserResult User,
    string AccessToken,
    DateTime ExpiresAt
);
