using System.Net.Http;
using Altinn.ApiClients.Maskinporten.Models;

namespace Altinn.ApiClients.Maskinporten.Extensions
{
    public static class HttpRequestMessageExtensions
    {
        private static readonly HttpRequestOptionsKey<MaskinportenRequestContext> MaskinportenRequestContextKey =
            new("Altinn.ApiClients.Maskinporten.RequestContext");

        public static HttpRequestMessage SetMaskinportenRequestContext(this HttpRequestMessage request,
            MaskinportenRequestContext requestContext)
        {
            request.Options.Set(MaskinportenRequestContextKey, requestContext);
            return request;
        }

        public static MaskinportenRequestContext GetMaskinportenRequestContext(this HttpRequestMessage request)
        {
            request.Options.TryGetValue(MaskinportenRequestContextKey, out var requestContext);
            return requestContext;
        }
    }
}
