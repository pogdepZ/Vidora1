using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Vidora.Core.Contracts.Services;

namespace Vidora.Infrastructure.Api.Handlers;

public sealed class AuthTokenHandler : DelegatingHandler
{
    private readonly ISessionStateService _sessionState;

    public AuthTokenHandler(ISessionStateService sessionState)
    {
        _sessionState = sessionState;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // ONLY attach token if exists
        if (_sessionState.IsAuthenticated)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue(
                scheme: _sessionState.AccessToken.Scheme,
                parameter: _sessionState.AccessToken.Token
                );
        }

        return base.SendAsync(request, cancellationToken);
    }
}
