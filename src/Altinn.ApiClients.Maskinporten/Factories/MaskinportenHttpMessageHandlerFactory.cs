using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Altinn.ApiClients.Maskinporten.Config;
using Altinn.ApiClients.Maskinporten.Handlers;
using Altinn.ApiClients.Maskinporten.Helpers;
using Altinn.ApiClients.Maskinporten.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Altinn.ApiClients.Maskinporten.Factories
{
    public class MaskinportenHttpMessageHandlerFactory
    {
        private readonly IEnumerable<IClientDefinition> _clientDefinitions;
        private readonly IMaskinportenService _maskinportenService;

        public MaskinportenHttpMessageHandlerFactory(IEnumerable<IClientDefinition> clientDefinitions,
            IMaskinportenService maskinportenService)
        {
            _clientDefinitions = clientDefinitions;
            _maskinportenService = maskinportenService;
        }

        public DelegatingHandler Get<TClientDefinition, THttpClient>(Action<TClientDefinition> configureClientDefinition = null)
            where TClientDefinition : class, IClientDefinition
        {
            return GetByKey(ClientDefinitionsHelper.GetKey<THttpClient>(), configureClientDefinition);
        }

        public DelegatingHandler Get<TClientDefinition>(string httpClientName, Action<TClientDefinition> configureClientDefinition = null)
            where TClientDefinition : class, IClientDefinition
        {
            return GetByKey(ClientDefinitionsHelper.GetKey(httpClientName), configureClientDefinition);
        }

        private DelegatingHandler GetByKey<TClientDefinition>(string key, Action<TClientDefinition> configureClientDefinition = null)
            where TClientDefinition : class, IClientDefinition
        {
            var index = ClientDefinitionsHelper.GetIndexOf(key);
            var clientDefinition = (TClientDefinition)_clientDefinitions.ElementAt(index);
            var settings = new MaskinportenSettings();
            ClientDefinitionsHelper.GetConfiguration(index).Bind(settings);
            clientDefinition.ClientSettings = settings;
            configureClientDefinition?.Invoke(clientDefinition);
            return new MaskinportenTokenHandler(_maskinportenService, clientDefinition);
        }
    }
}
