using System.Collections.Generic;
using System.Linq;
using Altinn.ApiClients.Maskinporten.Interfaces;

namespace Altinn.ApiClients.Maskinporten.Helpers
{
    /// <summary>
    /// Manage two lists with identifiers for instances of IClientDefinitions. The index of a given instance key
    /// in each of these lists are used by MaskinportenHttpMessageHandlerFactory to find the correct
    /// IClientDefinition instance in which to inject the correct settings.
    /// </summary>
    public static class MaskinportenClientDefinitionHelper
    {
        private static readonly List<string> ClientDefinitionInstanceKeys = new();
        private static readonly List<IMaskinportenSettings> Settings = new();

        public static void AddClientDefinitionInstance(string clientDefinitionKey, IMaskinportenSettings settings)
        {
            ClientDefinitionInstanceKeys.Add(clientDefinitionKey);
            Settings.Add(settings);
        }

        public static int GetIndexOf(string clientDefinitionKey)
        {
            return ClientDefinitionInstanceKeys.IndexOf(clientDefinitionKey);
        }

        public static IMaskinportenSettings GetSettingsByIndex(int index)
        {
            return Settings.ElementAt(index);
        }
    }
}
