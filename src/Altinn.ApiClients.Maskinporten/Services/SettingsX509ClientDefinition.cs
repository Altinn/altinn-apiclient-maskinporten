using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Altinn.ApiClients.Maskinporten.Config;
using Altinn.ApiClients.Maskinporten.Interfaces;
using Altinn.ApiClients.Maskinporten.Models;
using Microsoft.Extensions.Options;

namespace Altinn.ApiClients.Maskinporten.Services
{
    public class SettingsX509ClientDefinition : IClientDefinition
    {
        public SettingsX509ClientDefinition()
        {
        }

        public SettingsX509ClientDefinition(IOptions<MaskinportenSettings<SettingsJwkClientDefinition>> clientSettings)
        {
            ClientSettings = clientSettings.Value;
        }

        public SettingsX509ClientDefinition(MaskinportenSettings clientSettings)
        {
            ClientSettings = clientSettings;
        }

        public MaskinportenSettings ClientSettings { get; set; }

        public async Task<ClientSecrets> GetClientSecrets()
        {
            ClientSecrets clientSecrets = new ClientSecrets();

            if (string.IsNullOrEmpty(ClientSettings.EncodedX509)) return clientSecrets;

            // See tip #5 at https://paulstovell.com/x509certificate2/
            var file = Path.Combine(Path.GetTempPath(), "altinn-apiclient-maskinporten-" + Guid.NewGuid());
            await File.WriteAllBytesAsync(file, Convert.FromBase64String(ClientSettings.EncodedX509));
            try
            {
                clientSecrets.ClientCertificate = new X509Certificate2(
                    file,
                    String.Empty,
                    X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet |
                    X509KeyStorageFlags.Exportable);

                return clientSecrets;
            }
            finally
            {
                File.Delete(file);
            }
        }
    }

    public class SettingsX509ClientDefinition<T> : CertificateStoreClientDefinition where T : IClientDefinition
    {
        public SettingsX509ClientDefinition(IOptions<MaskinportenSettings<SettingsJwkClientDefinition<T>>> clientSettings)
        {
            ClientSettings = clientSettings.Value;
        }

        public SettingsX509ClientDefinition(MaskinportenSettings clientSettings) : base(clientSettings)
        {
        }
    }
}