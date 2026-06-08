using Altinn.ApiClients.Maskinporten.Models;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Altinn.ApiClients.Maskinporten.Extensions;
using Altinn.ApiClients.Maskinporten.Interfaces;

namespace Altinn.ApiClients.Maskinporten.Handlers
{
    public class MaskinportenTokenHandler : DelegatingHandler
    {
        private readonly IMaskinportenService _maskinporten;
        private readonly IClientDefinition _clientDefinition;

        public MaskinportenTokenHandler(IMaskinportenService maskinporten, IClientDefinition clientDefinition)
        {
            _maskinporten = maskinporten;
            _clientDefinition = clientDefinition;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            TokenResponse tokenResponse;
            if (request.Headers.Authorization == null ||
                (_clientDefinition.ClientSettings.OverwriteAuthorizationHeader.HasValue &&
                _clientDefinition.ClientSettings.OverwriteAuthorizationHeader.Value))
            {
                var requestContext = request.GetMaskinportenTokenRequestContext();
                tokenResponse = await GetTokenResponse(requestContext, cancellationToken);
                if (tokenResponse != null) 
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);
                }
            }

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode != HttpStatusCode.Unauthorized) 
            {
                return response;
            }

            tokenResponse = await RefreshTokenResponse(request.GetMaskinportenTokenRequestContext(), cancellationToken);
            if (tokenResponse != null)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);
                response = await base.SendAsync(request, cancellationToken);
            }

            return response;
        }

        private async Task<TokenResponse> GetTokenResponse(MaskinportenRequestContext requestContext, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) return null;
            TokenResponse tokenResponse =  await _maskinporten.GetToken(_clientDefinition, requestContext);
            return tokenResponse;
        }

        private async Task<TokenResponse> RefreshTokenResponse(MaskinportenRequestContext requestContext, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) return null;
            TokenResponse tokenResponse = await _maskinporten.GetToken(_clientDefinition, requestContext, true);
            return tokenResponse;
        }
    }
}
