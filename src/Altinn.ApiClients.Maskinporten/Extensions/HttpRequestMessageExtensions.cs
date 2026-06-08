using System.Net.Http;
using Altinn.ApiClients.Maskinporten.Models;

namespace Altinn.ApiClients.Maskinporten.Extensions
{
    public static class HttpRequestMessageExtensions
    {
        private static readonly HttpRequestOptionsKey<MaskinportenRequestContext> MaskinportenTokenRequestContextKey =
            new("Altinn.ApiClients.Maskinporten.TokenRequestContext");

        public static HttpRequestMessage SetMaskinportenTokenRequestContext(this HttpRequestMessage request,
            MaskinportenRequestContext requestContext)
        {
            request.Options.Set(MaskinportenTokenRequestContextKey, requestContext);
            return request;
        }

        public static MaskinportenRequestContext GetMaskinportenTokenRequestContext(this HttpRequestMessage request)
        {
            request.Options.TryGetValue(MaskinportenTokenRequestContextKey, out MaskinportenRequestContext requestContext);
            return requestContext;
        }
    }
}
