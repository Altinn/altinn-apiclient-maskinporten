using System;
using Altinn.ApiClients.Maskinporten.Factories;
using Altinn.ApiClients.Maskinporten.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Altinn.ApiClients.Maskinporten.Extensions
{
    public static class HttpClientBuilderExtensions
    {
        public static IHttpClientBuilder AddMaskinportenHttpMessageHandler<TClientDefinition>(
            this IHttpClientBuilder clientBuilder, string httpClientName, Action<TClientDefinition> configureClientDefinition = null)
            where TClientDefinition : class, IClientDefinition
        {
            return clientBuilder.AddHttpMessageHandler(sp =>
            {
                var maskinportenHttpMessageHandlerFactory =
                    sp.GetRequiredService<MaskinportenHttpMessageHandlerFactory>();
                return maskinportenHttpMessageHandlerFactory.Get(httpClientName, configureClientDefinition);
            });
        }

        public static IHttpClientBuilder AddMaskinportenHttpMessageHandler<TClientDefinition, THttpClient>(
            this IHttpClientBuilder clientBuilder, Action<TClientDefinition> configureClientDefinition = null)
            where TClientDefinition : class, IClientDefinition
            where THttpClient : class
        {
            return clientBuilder.AddHttpMessageHandler(sp =>
            {
                var maskinportenHttpMessageHandlerFactory =
                    sp.GetRequiredService<MaskinportenHttpMessageHandlerFactory>();
                return maskinportenHttpMessageHandlerFactory.Get<TClientDefinition, THttpClient>(configureClientDefinition);
            });
        }
    }
}
