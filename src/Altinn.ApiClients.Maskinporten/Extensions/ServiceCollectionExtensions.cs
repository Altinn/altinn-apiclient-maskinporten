using System;
using System.Linq;
using System.Net.Http;
using Altinn.ApiClients.Maskinporten.Config;
using Altinn.ApiClients.Maskinporten.Handlers;
using Altinn.ApiClients.Maskinporten.Interfaces;
using Altinn.ApiClients.Maskinporten.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Altinn.ApiClients.Maskinporten.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds a named Maskinporten-enabled HTTP client. Assumes a MaskinportenSettings&lt;TClientDefinition&gt; has been configured.
        /// </summary>
        /// <typeparam name="TClientDefinition">The client definition</typeparam>
        /// <param name="services">Service collection</param>
        /// <param name="httpClientName">Name of HTTP client</param>
        /// <param name="configureClientDefinition">Delegate for configuring the client definition</param>
        public static void AddMaskinportenHttpClient<TClientDefinition>(this IServiceCollection services, string httpClientName, Action<TClientDefinition> configureClientDefinition = null)
            where TClientDefinition : class, IClientDefinition, new() => AddMaskinportenHttpClient(services, httpClientName, null, configureClientDefinition);

        /// <summary>
        /// Adds a typed Maskinporten-enabled HTTP client. Assumes a MaskinportenSettings&lt;TClientDefinition, THttpClient&gt; has been configured.
        /// </summary>
        /// <typeparam name="TClientDefinition">The client definition</typeparam>
        /// <typeparam name="THttpClient">The HTTP client type</typeparam>
        /// <param name="services">Service collection</param>
        /// <param name="configureClientDefinition">Delegate for configuring the client definition</param>
        public static void AddMaskinportenHttpClient<TClientDefinition, THttpClient>(this IServiceCollection services, Action<TClientDefinition> configureClientDefinition = null)
            where TClientDefinition : class, IClientDefinition, new()
            where THttpClient : class => AddMaskinportenHttpClient<TClientDefinition, THttpClient>(services, null, configureClientDefinition);

        /// <summary>
        /// Adds a named Maskinporten-enabled HTTP client with the given configuration
        /// </summary>
        /// <typeparam name="TClientDefinition">The client definition</typeparam>
        /// <param name="services">Service collection</param>
        /// <param name="httpClientName">Name of HTTP client</param>
        /// <param name="config">Configuration to use</param>
        /// <param name="configureClientDefinition">Delegate for configuring the client definition</param>
        public static void AddMaskinportenHttpClient<TClientDefinition>(this IServiceCollection services, string httpClientName, IConfiguration config, Action<TClientDefinition> configureClientDefinition = null) 
            where TClientDefinition : class, IClientDefinition, new()
        {
            AddMaskinportenClientCommon(services);
            if (config != null)
            {
                // Add the configuration that will be injected for the requested client definition
                services.Configure<MaskinportenSettings<TClientDefinition>>(config);
            }

            services.AddHttpClient(httpClientName).AddHttpMessageHandler(sp => GetHttpMessageHandler(sp, configureClientDefinition));
        }

        /// <summary>
        /// Adds a typed Maskinporten-enabled HTTP client with the given configuration
        /// </summary>
        /// <typeparam name="TClientDefinition">The client definition</typeparam>
        /// <typeparam name="THttpClient">The HTTP client type</typeparam>
        /// <param name="services">Service collection</param>
        /// <param name="config">Configuration to use</param>
        /// <param name="configureClientDefinition">Delegate for configuring the client definition</param>
        public static void AddMaskinportenHttpClient<TClientDefinition, THttpClient>(this IServiceCollection services, IConfiguration config, Action<TClientDefinition> configureClientDefinition = null) 
            where TClientDefinition : class, IClientDefinition, new()
            where THttpClient : class 
        {
            AddMaskinportenClientCommon(services);
            if (config != null)
            {
                // Add the configuration that will be injected for the requested client definition
                services.Configure<MaskinportenSettings<TClientDefinition, THttpClient>>(config);
            }
            services.AddHttpClient<THttpClient>().AddHttpMessageHandler(sp => GetHttpMessageHandler<TClientDefinition, THttpClient>(sp, configureClientDefinition));
        }

        /// <summary>
        /// Returns a delegate handler for use with AddHttpMessageHandler and typed clients
        /// </summary>
        /// <typeparam name="TClientDefinition">The client definition</typeparam>
        /// <typeparam name="THttpClient">The typed http client</typeparam>
        /// <param name="sp">Service provider</param>
        /// <param name="configureClientDefinition">Delegate for configuring the client definition</param>
        /// <returns>A http message handler</returns>
        private static DelegatingHandler GetHttpMessageHandler<TClientDefinition, THttpClient>(IServiceProvider sp, Action<TClientDefinition> configureClientDefinition = null)
            where TClientDefinition : class, IClientDefinition, new()
            where THttpClient : class
        {
            var clientSettings = sp.GetRequiredService<IOptions<MaskinportenSettings<TClientDefinition, THttpClient>>>().Value;
            var clientDefinition = new TClientDefinition
            {
                ClientSettings = clientSettings
            };
            configureClientDefinition?.Invoke(clientDefinition);

            return new MaskinportenTokenHandler(sp.GetRequiredService<IMaskinportenService>(), clientDefinition);
        }

        /// <summary>
        /// Returns a delegate handler for use with AddHttpMessageHandler and named clients
        /// </summary>
        /// <typeparam name="TClientDefinition"></typeparam>
        /// <param name="sp">Service provider</param>
        /// <param name="configureClientDefinition">Delegate for configuring the client definition</param>
        /// <returns>A http message handler</returns>
        private static DelegatingHandler GetHttpMessageHandler<TClientDefinition>(IServiceProvider sp, Action<TClientDefinition> configureClientDefinition = null)
            where TClientDefinition : class, IClientDefinition, new()
        {
            var clientSettings = sp.GetRequiredService<IOptions<MaskinportenSettings<TClientDefinition>>>().Value;
            var clientDefinition = new TClientDefinition
            {
                ClientSettings = clientSettings
            };
            configureClientDefinition?.Invoke(clientDefinition);

            return new MaskinportenTokenHandler(sp.GetRequiredService<IMaskinportenService>(), clientDefinition);
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
        }
    }
}
