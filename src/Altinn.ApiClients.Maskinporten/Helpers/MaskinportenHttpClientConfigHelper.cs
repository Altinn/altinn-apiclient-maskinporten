using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Altinn.ApiClients.Maskinporten.Helpers
{
    public static class MaskinportenHttpClientConfigHelper
    {
        public static readonly List<string> HttpClientKeys = new();
        public static readonly List<IConfiguration> Configurations = new();

        private const string TypedPrefix = "__typed__";
        private const string NamedPrefix = "__named__";

        public static string GetKeyForHttpClient<T>()
        {
            return TypedPrefix + typeof(T).FullName;
        }

        public static string GetKeyForHttpClient(string name)
        {
            return NamedPrefix + name;
        }

        public static void AddHttpClientConfiguration<THttpClient>(IConfiguration configuration)
        {
            InternalAddHttpClientKeyAndConfiguration(GetKeyForHttpClient<THttpClient>(), configuration);
        }

        public static void AddHttpClientConfiguration(string httpClientName, IConfiguration configuration)
        {
            InternalAddHttpClientKeyAndConfiguration(GetKeyForHttpClient(httpClientName), configuration);
        }

        public static int GetIndexOf(string key)
        {
            return HttpClientKeys.IndexOf(key);
        }

        public static IConfiguration GetConfiguration(int index)
        {
            return Configurations.ElementAt(index);
        }

        private static void InternalAddHttpClientKeyAndConfiguration(string key, IConfiguration configuration)
        {
            HttpClientKeys.Add(key);
            Configurations.Add(configuration);
        }
    }
}
