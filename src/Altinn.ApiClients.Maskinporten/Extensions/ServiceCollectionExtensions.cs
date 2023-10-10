using System;
using System.Linq;
using Altinn.ApiClients.Maskinporten.Config;
using Altinn.ApiClients.Maskinporten.Factories;
using Altinn.ApiClients.Maskinporten.Helpers;
using Altinn.ApiClients.Maskinporten.Interfaces;
using Altinn.ApiClients.Maskinporten.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Altinn.ApiClients.Maskinporten.Extensions
{
    /// <summary>
    /// We add all IClientDefinition implementation to the DI container and immediately add a reference to the corresponding
    /// httpClient and configuration to a static list via the helpers in MaskinportenClientDefinitionHelper.
    /// The MaskinportenHttpMessageHandlerFactory relies on the index of a given httpClient refence/configuration
    /// matching the index of the IClientDefinition service when injected as a IEnumerable&lt;IClientDefinition&gt;
    /// This way we can support having multiple named/typed HTTP clients using the same IClientDefinition implementation,
    /// (but different singleton instances), whilst the IClientDefinition can use normal DI in its constructors
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers a Maskinporten client definition instance with a unique key. This allows for several instances using the
        /// same client definition but varying configurations. Use IHttpClientBuilder.AddMaskinportenHttpMessageHandler
        /// to attach it to a HttpClient, using the same key.
        /// </summary>
        /// <typeparam name="TClientDefinition">The client definition</typeparam>
        /// <param name="services">Service collection</param>
        /// <param name="clientDefinitionKey">Key to uniquely identify this client definition instance</param>
        /// <param name="settings">Instance of IMaskinportenSettings to use</param>
        public static void RegisterMaskinportenClientDefinition<TClientDefinition>(this IServiceCollection services,
            string clientDefinitionKey, IMaskinportenSettings settings)
            where TClientDefinition : class, IClientDefinition
        {
            AddMaskinportenClientCommon(services);
            services.AddSingleton<IClientDefinition, TClientDefinition>();
            MaskinportenClientDefinitionHelper.AddClientDefinitionInstance(clientDefinitionKey, settings);
        }        
        
        /// <summary>
        /// Registers a Maskinporten client definition instance with a unique key. This allows for several instances using the
        /// same client definition but varying configurations. Use IHttpClientBuilder.AddMaskinportenHttpMessageHandler
        /// to attach it to a HttpClient, using the same key.
        /// </summary>
        /// <typeparam name="TClientDefinition">The client definition</typeparam>
        /// <param name="services">Service collection</param>
        /// <param name="clientDefinitionKey">Key to uniquely identify this client definition instance</param>
        /// <param name="config">IConfiguration instance containing fields that will be bound to MaskinportenSettings</param>
        public static void RegisterMaskinportenClientDefinition<TClientDefinition>(this IServiceCollection services,
            string clientDefinitionKey, IConfiguration config)
            where TClientDefinition : class, IClientDefinition =>
            RegisterMaskinportenClientDefinition<TClientDefinition>(services, clientDefinitionKey, BindConfigToInstanceOf<MaskinportenSettings>(config));

        /// <summary>
        /// Adds a named Maskinporten-enabled HTTP client with the given configuration which can be created with HttpClientFactory
        /// </summary>
        /// <typeparam name="TClientDefinition">The client definition</typeparam>
        /// <param name="services">Service collection</param>
        /// <param name="httpClientName">Name of HTTP client.</param>
        /// <param name="settings">Instance of IMaskinportenSettings to use</param>
        /// <param name="configureClientDefinition">Delegate for configuring the client definition</param>
        public static IHttpClientBuilder AddMaskinportenHttpClient<TClientDefinition>(this IServiceCollection services,
            string httpClientName, IMaskinportenSettings settings, Action<TClientDefinition> configureClientDefinition = null)
            where TClientDefinition : class, IClientDefinition
        {
            services.RegisterMaskinportenClientDefinition<TClientDefinition>(httpClientName, settings);
            return services.AddHttpClient(httpClientName)
                .AddMaskinportenHttpMessageHandler(httpClientName, configureClientDefinition);
        }

        /// <summary>
        /// Adds a named Maskinporten-enabled HTTP client with the given configuration which can be created with HttpClientFactory
        /// </summary>
        /// <typeparam name="TClientDefinition">The client definition</typeparam>
        /// <param name="services">Service collection</param>
        /// <param name="httpClientName">Name of HTTP client.</param>
        /// <param name="config">IConfiguration instance containing fields that will be bound to MaskinportenSettings</param>
        /// <param name="configureClientDefinition">Delegate for configuring the client definition</param>
        public static IHttpClientBuilder AddMaskinportenHttpClient<TClientDefinition>(this IServiceCollection services,
            string httpClientName, IConfiguration config, Action<TClientDefinition> configureClientDefinition = null)
            where TClientDefinition : class, IClientDefinition =>
            AddMaskinportenHttpClient(services, httpClientName, BindConfigToInstanceOf<MaskinportenSettings>(config), configureClientDefinition);

        /// <summary>
        /// Adds a typed Maskinporten-enabled HTTP client with the given configuration to the dependency injection container
        /// </summary>
        /// <typeparam name="TClientDefinition">The client definition</typeparam>
        /// <typeparam name="THttpClient">The HTTP client type</typeparam>
        /// <param name="services">Service collection</param>
        /// <param name="settings">Instance of IMaskinportenSettings to use</param>
        /// <param name="configureClientDefinition">Delegate for configuring the client definition</param>
        public static IHttpClientBuilder AddMaskinportenHttpClient<TClientDefinition, THttpClient>(this IServiceCollection services,
            IMaskinportenSettings settings, Action<TClientDefinition> configureClientDefinition = null)
            where TClientDefinition : class, IClientDefinition
            where THttpClient : class
        {
            services.RegisterMaskinportenClientDefinition<TClientDefinition>(
                MaskinportenClientDefinitionHelper.GetClientDefinitionKey<THttpClient>(), settings);

            return services.AddHttpClient<THttpClient>()
                .AddMaskinportenHttpMessageHandler<TClientDefinition, THttpClient>(configureClientDefinition);
        }

        /// <summary>
        /// Adds a typed Maskinporten-enabled HTTP client with the given configuration to the dependency injection container
        /// </summary>
        /// <typeparam name="TClientDefinition">The client definition</typeparam>
        /// <typeparam name="THttpClient">The HTTP client type</typeparam>
        /// <typeparam name="THttpClientImplementation">The HTTP client implementation type</typeparam>
        /// <param name="services">Service collection</param>
        /// <param name="settings">Instance of IMaskinportenSettings to use</param>
        /// <param name="configureClientDefinition">Delegate for configuring the client definition</param>
        public static IHttpClientBuilder AddMaskinportenHttpClient<TClientDefinition, THttpClient, THttpClientImplementation>(this IServiceCollection services,
            IMaskinportenSettings settings, Action<TClientDefinition> configureClientDefinition = null)
            where TClientDefinition : class, IClientDefinition
            where THttpClient : class
            where THttpClientImplementation : class, THttpClient
        {
            services.RegisterMaskinportenClientDefinition<TClientDefinition>(
                MaskinportenClientDefinitionHelper.GetClientDefinitionKey<THttpClient>(), settings);

            return services.AddHttpClient<THttpClient, THttpClientImplementation>()
                .AddMaskinportenHttpMessageHandler<TClientDefinition, THttpClient>(configureClientDefinition);
        }

        /// <summary>
        /// Adds a typed Maskinporten-enabled HTTP client with the given configuration to the dependency injection container
        /// </summary>
        /// <typeparam name="TClientDefinition">The client definition</typeparam>
        /// <typeparam name="THttpClient">The HTTP client type</typeparam>
        /// <typeparam name="THttpClientImplementation">The HTTP client implementation type</typeparam>
        /// <param name="services">Service collection</param>
        /// <param name="config">IConfiguration instance containing fields that will be bound to MaskinportenSettings</param>
        /// <param name="configureClientDefinition">Delegate for configuring the client definition</param>
        public static IHttpClientBuilder AddMaskinportenHttpClient<TClientDefinition, THttpClient, THttpClientImplementation>(this IServiceCollection services,
            IConfiguration config, Action<TClientDefinition> configureClientDefinition = null)
            where TClientDefinition : class, IClientDefinition
            where THttpClient : class
            where THttpClientImplementation : class, THttpClient =>
            AddMaskinportenHttpClient<TClientDefinition, THttpClient, THttpClientImplementation>(services,
                BindConfigToInstanceOf<MaskinportenSettings>(config), configureClientDefinition);

        /// <summary>
        /// Adds a typed Maskinporten-enabled HTTP client with the given configuration to the dependency injection container
        /// </summary>
        /// <typeparam name="TClientDefinition">The client definition</typeparam>
        /// <typeparam name="THttpClient">The HTTP client type</typeparam>
        /// <param name="services">Service collection</param>
        /// <param name="config">IConfiguration instance containing fields that will be bound to MaskinportenSettings</param>
        /// <param name="configureClientDefinition">Delegate for configuring the client definition</param>
        public static IHttpClientBuilder AddMaskinportenHttpClient<TClientDefinition, THttpClient>(this IServiceCollection services,
            IConfiguration config, Action<TClientDefinition> configureClientDefinition = null)
            where TClientDefinition : class, IClientDefinition
            where THttpClient : class =>
            AddMaskinportenHttpClient<TClientDefinition, THttpClient>(services,
                BindConfigToInstanceOf<MaskinportenSettings>(config), configureClientDefinition);

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

        private static TBoundInstance BindConfigToInstanceOf<TBoundInstance>(IConfiguration configuration)
            where TBoundInstance : class, new()
        {
            var boundInstance = new TBoundInstance();
            configuration.Bind(boundInstance);
            return boundInstance;
        }
    }
}
