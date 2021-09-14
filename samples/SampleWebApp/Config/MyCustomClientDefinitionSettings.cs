using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Altinn.ApiClients.Maskinporten.Config;

namespace SampleWebApp.Config
{
    public class MyCustomClientDefinitionSettings : MaskinportenSettings
    {
        public string AzureKeyVaultName { get; set; }
        public string SecretName { get; set; }
    }
}
