using System.Linq;
using Altinn.ApiClients.Maskinporten.Config;
using Altinn.ApiClients.Maskinporten.Handlers;
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
            // We need a provider to cache tokens. If one is not already provided by the user, use MemoryTokenCacheProvider
            if (services.All(x => x.ServiceType != typeof(ITokenCacheProvider)))
            {
                services.AddMemoryCache();
                services.TryAddSingleton<ITokenCacheProvider, MemoryTokenCacheProvider>();
            }

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
