using Altinn.ApiClients.Maskinporten.Models;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Altinn.ApiClients.Maskinporten.Interfaces;

namespace Altinn.ApiClients.Maskinporten.Handlers
{
    public class MaskinportenTokenHandler<T> : DelegatingHandler where T : IClientDefinition
    {
        private readonly IMaskinportenService _maskinporten;
        private readonly T _clientDefinition;

        public MaskinportenTokenHandler(IMaskinportenService maskinporten, T clientDefinition)
        {
            _maskinporten = maskinporten;
            _clientDefinition = clientDefinition;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Headers.Authorization == null)
            {
                TokenResponse tokenResponse = await GetTokenResponse(cancellationToken);
                if (tokenResponse != null)
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);
            }

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode != HttpStatusCode.Unauthorized) return response;
            {
                TokenResponse tokenResponse = await RefreshTokenResponse(cancellationToken);
                if (tokenResponse != null)
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);
                    response = await base.SendAsync(request, cancellationToken);
                }
            }

            return response;
        }

        private async Task<TokenResponse> GetTokenResponse(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) return null;
            TokenResponse tokenResponse =  await _maskinporten.GetToken(_clientDefinition);
            return tokenResponse;
        }

        private async Task<TokenResponse> RefreshTokenResponse(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) return null;
            TokenResponse tokenResponse = await _maskinporten.GetToken(_clientDefinition, true);
            return tokenResponse;
        }
    }
}
