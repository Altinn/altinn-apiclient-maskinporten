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
    public class Pkcs12ClientDefinition : IClientDefinition
    {
        public MaskinportenSettings ClientSettings { get; set; }

        public Pkcs12ClientDefinition()
        {
        }

        public Pkcs12ClientDefinition(IOptions<MaskinportenSettings<Pkcs12ClientDefinition>> clientSettings)
        {
            ClientSettings = clientSettings.Value;
        }

        public Pkcs12ClientDefinition(MaskinportenSettings clientSettings)
        {
            ClientSettings = clientSettings;
        }

        public Task<ClientSecrets> GetClientSecrets()
        {
            string p12KeyStoreFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ClientSettings.CertificatePkcs12Path);

            X509Certificate2 signingCertificate = new X509Certificate2(
                p12KeyStoreFile, 
                ClientSettings.CertificatePkcs12Password, 
                X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);

            return Task.FromResult(new ClientSecrets()
            {
                ClientCertificate = signingCertificate
            });
        }
    }

    public class Pkcs12ClientDefinition<T> : Pkcs12ClientDefinition where T : IClientDefinition
    {
        public Pkcs12ClientDefinition(IOptions<MaskinportenSettings<Pkcs12ClientDefinition<T>>> clientSettings)
        {
            ClientSettings = clientSettings.Value;
        }

        public Pkcs12ClientDefinition(MaskinportenSettings clientSettings) : base(clientSettings)
        {
        }
    }
}
