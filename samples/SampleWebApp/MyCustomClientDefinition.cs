using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Altinn.ApiClients.Maskinporten.Config;
using Altinn.ApiClients.Maskinporten.Interfaces;
using Altinn.ApiClients.Maskinporten.Models;
using Azure.Security.KeyVault.Secrets;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using SampleWebApp.Config;

namespace SampleWebApp
{
    public class MyCustomClientDefinition : IClientDefinition
    {
        private readonly ILogger<MyCustomClientDefinition> _logger;
        public MaskinportenSettings ClientSettings { get; set; }
        public MyCustomClientDefinitionSettings MyCustomClientDefinitionSettings { get; set; } = new();

        private ClientSecrets _clientSecrets;

        public MyCustomClientDefinition(ILogger<MyCustomClientDefinition> logger)
        {
            _logger = logger;
        }

        public async Task<ClientSecrets> GetClientSecrets()
        {
            if (_clientSecrets != null)
            {
                return _clientSecrets;
            }

            _logger.LogInformation("Getting secrets from Azure");
            
            var secretClient = new SecretClient(
                new Uri($"https://{MyCustomClientDefinitionSettings.AzureKeyVaultName}.vault.azure.net/"),
                new DefaultAzureCredential());

            var secret = await secretClient.GetSecretAsync(MyCustomClientDefinitionSettings.SecretName);
            var base64Str = secret.Value.ToString();
            if (base64Str == null)
            {
                throw new ApplicationException("Unable to fetch cert from key vault");
            }

            var signingCertificate = new X509Certificate2(
                Convert.FromBase64String(base64Str),
                ClientSettings.CertificatePkcs12Password,
                X509KeyStorageFlags.EphemeralKeySet);

            _clientSecrets =  new ClientSecrets()
            {
                ClientCertificate = signingCertificate
            };

            return _clientSecrets;
        }
    }
}
