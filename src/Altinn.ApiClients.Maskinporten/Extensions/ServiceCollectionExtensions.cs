using System;
using System.Linq;
using Altinn.ApiClients.Maskinporten.Factories;
using Altinn.ApiClients.Maskinporten.Helpers;
using Altinn.ApiClients.Maskinporten.Interfaces;
using Altinn.ApiClients.Maskinporten.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Altinn.ApiClients.Maskinporten.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds a named Maskinporten-enabled HTTP client with the given configuration
        /// </summary>
        /// <typeparam name="TClientDefinition">The client definition</typeparam>
        /// <param name="services">Service collection</param>
        /// <param name="httpClientName">Name of HTTP client</param>
        /// <param name="config">Configuration to use</param>
        /// <param name="configureClientDefinition">Delegate for configuring the client definition</param>
        public static void AddMaskinportenHttpClient<TClientDefinition>(this IServiceCollection services,
            string httpClientName, IConfiguration config, Action<TClientDefinition> configureClientDefinition = null)
            where TClientDefinition : class, IClientDefinition
        {
            AddMaskinportenClientCommon(services);
            services.AddSingleton<IClientDefinition, TClientDefinition>();
            ClientDefinitionsHelper.Add(httpClientName, config);

            services.AddHttpClient(httpClientName).AddHttpMessageHandler(sp =>
            {
                var factory = sp.GetRequiredService<MaskinportenHttpMessageHandlerFactory>();
                return factory.Get(httpClientName, configureClientDefinition);
            });
        }

        /// <summary>
        /// Adds a typed Maskinporten-enabled HTTP client with the given configuration
        /// </summary>
        /// <typeparam name="TClientDefinition">The client definition</typeparam>
        /// <typeparam name="THttpClient">The HTTP client type</typeparam>
        /// <param name="services">Service collection</param>
        /// <param name="config">Configuration to use</param>
        /// <param name="configureClientDefinition">Delegate for configuring the client definition</param>
        public static void AddMaskinportenHttpClient<TClientDefinition, THttpClient>(this IServiceCollection services,
            IConfiguration config, Action<TClientDefinition> configureClientDefinition = null)
            where TClientDefinition : class, IClientDefinition
            where THttpClient : class
        {
            AddMaskinportenClientCommon(services);
            services.AddSingleton<IClientDefinition, TClientDefinition>();
            ClientDefinitionsHelper.Add<THttpClient>(config);
            services.AddHttpClient<THttpClient>().AddHttpMessageHandler(sp =>
            {
                var factory = sp.GetRequiredService<MaskinportenHttpMessageHandlerFactory>();
                return factory.Get<TClientDefinition, THttpClient>(configureClientDefinition);
            });
        }

        private static void AddMaskinportenClientCommon(IServiceCollection services)
        {
            // We need a provider to cache tokens. If one is not already provided by the user, use MemoryTokenCacheProvider
            if (services.All(x => x.ServiceType != typeof(ITokenCacheProvider)))
            {
                services.AddMemoryCache();
                services.TryAddSingleton<ITokenCacheProvider, MemoryTokenCacheProvider>();
            }

            // We only need a single Maskinporten-service for all clients. This can be used directly if low level access is required.
            services.TryAddSingleton<IMaskinportenService, MaskinportenService>();
            services.TryAddSingleton<MaskinportenHttpMessageHandlerFactory>();
        }
    }
}
