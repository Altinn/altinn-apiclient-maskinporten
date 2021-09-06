using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Altinn.ApiClients.Maskinporten.Config;
using Altinn.ApiClients.Maskinporten.Models;
using Altinn.ApiClients.Maskinporten.Service;
using Microsoft.Extensions.Options;

namespace SampleWebApp.Service
{
    public class MyCustomClientSecretService : IClientSecret<IMyCustomClientSecretService>
    {
        private MaskinportenSettings _maskinportenSettings;

        public MyCustomClientSecretService(IOptions<MaskinportenSettings> maskinportenSettings)
        {
            _maskinportenSettings = maskinportenSettings.Value;
        }

        public Task<ClientSecrets> GetClientSecrets()
        {
            throw new NotImplementedException();
        }
    }
}
