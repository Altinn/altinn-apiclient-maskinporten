using Altinn.ApiClients.Maskinporten.Config;
using Altinn.ApiClients.Maskinporten.Handlers;
using Altinn.ApiClients.Maskinporten.Interfaces;
using Altinn.ApiClients.Maskinporten.Services;
using Microsoft.Extensions.Caching.Memory;
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
        /// <typeparam name="T"></typeparam>
        /// <param name="services"></param>
        /// <param name="config"></param>
        /// <param name="httpClientName"></param>
        public static void AddMaskinportenHttpClient<T>(this IServiceCollection services, IConfiguration config, string httpClientName) 
            where T : class, IClientDefinition
        {
            AddMaskinportenClientCommon<T>(services, config);
            services.AddHttpClient(httpClientName).AddHttpMessageHandler<MaskinportenTokenHandler<T>>();
        }

        /// Adds a typed Maskinporten-enabled HTTP client with the given configuration
        public static void AddMaskinportenHttpClient<T, T2>(this IServiceCollection services, IConfiguration config) 
            where T : class, IClientDefinition 
            where T2 : class 
        {
            AddMaskinportenClientCommon<T>(services, config);
            services.AddHttpClient<T2>().AddHttpMessageHandler<MaskinportenTokenHandler<T>>();
        }

        private static void AddMaskinportenClientCommon<T>(IServiceCollection services, IConfiguration config)
            where T : class, IClientDefinition
        {
            // Maskinporten requires a memory cache implementation
            services.TryAddSingleton<IMemoryCache, MemoryCache>();

            // We also need at least one HTTP client in order to fetch tokens
            //            services.AddHttpClient();

            // We only need a single Maskinporten-service for all clients. This can be used directly if low level access is required.
            services.TryAddSingleton<IMaskinportenService, MaskinportenService>();

            // Add the configuration that will be injected for the requested client definition
            services.Configure<MaskinportenSettings<T>>(config);

            // Add the client definition
            services.TryAddSingleton<T>();

            // Add a handler for the requested definition
            services.TryAddTransient<MaskinportenTokenHandler<T>>();
        }
    }
}
