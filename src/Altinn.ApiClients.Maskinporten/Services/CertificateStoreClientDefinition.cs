using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Altinn.ApiClients.Maskinporten.Config;
using Altinn.ApiClients.Maskinporten.Interfaces;
using Altinn.ApiClients.Maskinporten.Models;
using Microsoft.Extensions.Options;

namespace Altinn.ApiClients.Maskinporten.Services
{
    public class CertificateStoreClientDefinition : IClientDefinition
    {
        public MaskinportenSettings ClientSettings { get; set; }

        public CertificateStoreClientDefinition()
        {
        }

        public CertificateStoreClientDefinition(IOptions<MaskinportenSettings<CertificateStoreClientDefinition>> clientSettings)
        {
            ClientSettings = clientSettings.Value;
        }

        public CertificateStoreClientDefinition(MaskinportenSettings clientSettings)
        {
            ClientSettings = clientSettings;
        }

        public Task<ClientSecrets> GetClientSecrets()
        {
            X509Certificate2 signingCertificate = GetCertificateFromKeyStore(ClientSettings.CertificateStoreThumbprint, StoreName.My, StoreLocation.LocalMachine);
            return Task.FromResult(new ClientSecrets()
            {
                ClientCertificate = signingCertificate
            });
        }

        private static X509Certificate2 GetCertificateFromKeyStore(string thumbprint, StoreName storeName, StoreLocation storeLocation, bool onlyValid = false)
        {
            var store = new X509Store(storeName, storeLocation);
            store.Open(OpenFlags.ReadOnly);
            var certCollection = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, onlyValid);
            var enumerator = certCollection.GetEnumerator();
            X509Certificate2 cert = null;
            while (enumerator.MoveNext())
            {
                cert = enumerator.Current;
            }

            if (cert == null)
            {
                throw new ArgumentException("Unable to find certificate in store with thumbprint: " + thumbprint + ". Check your config, and make sure the certificate is installed in the \"LocalMachine\\My\" store.");
            }

            return cert;
        }
    }

    public class CertificateStoreClientDefinition<T> : CertificateStoreClientDefinition where T : IClientDefinition
    {
        public CertificateStoreClientDefinition(IOptions<MaskinportenSettings<CertificateStoreClientDefinition<T>>> clientSettings)
        {
            ClientSettings = clientSettings.Value;
        }

        public CertificateStoreClientDefinition(MaskinportenSettings clientSettings) : base(clientSettings)
        {
        }
    }
}
