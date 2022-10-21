using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Altinn.ApiClients.Maskinporten.Helpers
{
    public static class ClientDefinitionsHelper
    {
        public static readonly List<string> ClientDefinitions = new();
        public static readonly List<IConfiguration> Configurations = new();

        private const string TypedPrefix = "__typed__";
        private const string NamedPrefix = "__named__";

        public static string GetKey<T>()
        {
            return TypedPrefix + typeof(T).FullName;
        }

        public static string GetKey(string name)
        {
            return NamedPrefix + name;
        }

        public static void Add<THttpClient>(IConfiguration configuration)
        {
            InternalAddClientDefinitionAndConfiguration(GetKey<THttpClient>(), configuration);
        }

        public static void Add(string httpClientName, IConfiguration configuration)
        {
            InternalAddClientDefinitionAndConfiguration(GetKey(httpClientName), configuration);
        }

        public static int GetIndexOf(string key)
        {
            return ClientDefinitions.IndexOf(key);
        }

        public static IConfiguration GetConfiguration(int index)
        {
            return Configurations.ElementAt(index);
        }

        private static void InternalAddClientDefinitionAndConfiguration(string key, IConfiguration configuration)
        {
            ClientDefinitions.Add(key);
            Configurations.Add(configuration);
        }
    }
}
