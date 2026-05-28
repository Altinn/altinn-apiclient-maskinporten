using System.Net.Http;
using Altinn.ApiClients.Maskinporten.Models;

namespace Altinn.ApiClients.Maskinporten.Extensions
{
    public static class HttpRequestMessageExtensions
    {
        private static readonly HttpRequestOptionsKey<MaskinportenTokenRequestContext> MaskinportenTokenRequestContextKey =
            new("Altinn.ApiClients.Maskinporten.TokenRequestContext");

        public static HttpRequestMessage SetMaskinportenTokenRequestContext(this HttpRequestMessage request,
            MaskinportenTokenRequestContext requestContext)
        {
            request.Options.Set(MaskinportenTokenRequestContextKey, requestContext);
            return request;
        }

        public static MaskinportenTokenRequestContext GetMaskinportenTokenRequestContext(this HttpRequestMessage request)
        {
            request.Options.TryGetValue(MaskinportenTokenRequestContextKey, out MaskinportenTokenRequestContext requestContext);
            return requestContext;
        }
    }
}
