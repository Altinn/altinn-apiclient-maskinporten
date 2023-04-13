using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Altinn.ApiClients.Maskinporten.Config;
using Altinn.ApiClients.Maskinporten.Interfaces;
using Altinn.ApiClients.Maskinporten.Models;

namespace Altinn.ApiClients.Maskinporten.Services
{
    public class Pkcs12ClientDefinition : IClientDefinition
    {
        public IMaskinportenSettings ClientSettings { get; set; }

        public Task<ClientSecrets> GetClientSecrets()
        {
            string p12KeyStoreFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ClientSettings.CertificatePkcs12Path);

            X509Certificate2 signingCertificate = new X509Certificate2(
                p12KeyStoreFile, 
                ClientSettings.CertificatePkcs12Password,
                X509KeyStorageFlags.EphemeralKeySet);

            return Task.FromResult(new ClientSecrets()
            {
                ClientCertificate = signingCertificate
            });
        }
    }
}
